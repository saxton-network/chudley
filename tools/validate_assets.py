"""Validate Chudley's imported candidate sprites without image tooling.

Run from any directory: python tools/validate_assets.py
"""

from __future__ import annotations

import json
from pathlib import Path
import struct
import sys
import zlib


ROOT = Path(__file__).resolve().parents[1]
CANDIDATE = ROOT / "assets" / "sprites" / "candidate"
ATLAS = CANDIDATE / "chudley-atlas-4x6.png"
MANIFEST = CANDIDATE / "manifest.json"
QA = CANDIDATE / "qa.json"
WIDTH, HEIGHT = 512, 768
FRAME_SIZE = 128
COLUMNS, ROWS = 4, 6
EXPECTED_NAMES = (
    "front", "three_quarter_right", "three_quarter_left", "side_right",
    "reach_pocket", "grab_cheesy_puffs", "lift_cheesy_puffs", "eat_cheesy_puffs",
    "hold_red_can", "open_red_can_half_blink", "sip_red_can_closed_eyes",
    "lower_red_can_reopen", "rest", "move", "startle_move", "settle",
    "attention", "click", "shocked", "wave", "point", "smug_blue_lips",
    "amused_blue_lips", "signature_blue_lips",
)


class AssetError(ValueError):
    """The supplied candidate is incomplete or structurally inconsistent."""


def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssetError(message)


def read_png(path: Path) -> tuple[int, int, list[bytes]]:
    """Decode non-interlaced 8-bit RGBA PNG rows, including all PNG filters."""
    require(path.is_file(), f"missing PNG: {path}")
    data = path.read_bytes()
    require(data.startswith(b"\x89PNG\r\n\x1a\n"), f"invalid PNG signature: {path}")
    offset = 8
    header = None
    compressed = bytearray()
    ended = False
    while offset + 12 <= len(data):
        length = struct.unpack_from(">I", data, offset)[0]
        kind = data[offset + 4:offset + 8]
        end = offset + 12 + length
        require(end <= len(data), f"truncated PNG chunk: {path}")
        payload = data[offset + 8:offset + 8 + length]
        stored_crc = struct.unpack_from(">I", data, offset + 8 + length)[0]
        require(zlib.crc32(kind + payload) == stored_crc, f"PNG CRC mismatch: {path}")
        if kind == b"IHDR":
            require(header is None and length == 13, f"invalid PNG header: {path}")
            header = struct.unpack(">IIBBBBB", payload)
        elif kind == b"IDAT":
            compressed.extend(payload)
        elif kind == b"IEND":
            ended = True
            break
        offset = end
    require(header is not None and ended and compressed, f"incomplete PNG: {path}")
    width, height, depth, color, compression, filtering, interlace = header
    require((depth, color, compression, filtering, interlace) == (8, 6, 0, 0, 0),
            f"expected non-interlaced 8-bit RGBA PNG: {path}")
    stride = width * 4
    try:
        raw = zlib.decompress(compressed)
    except zlib.error as exc:
        raise AssetError(f"invalid PNG pixel data: {path}: {exc}") from exc
    require(len(raw) == height * (stride + 1), f"invalid PNG row data length: {path}")
    rows: list[bytes] = []
    prior = bytearray(stride)
    for y in range(height):
        start = y * (stride + 1)
        method = raw[start]
        require(method in range(5), f"unsupported PNG row filter {method}: {path}")
        row = bytearray(raw[start + 1:start + 1 + stride])
        for x in range(stride):
            left = row[x - 4] if x >= 4 else 0
            above = prior[x]
            upper_left = prior[x - 4] if x >= 4 else 0
            if method == 1:
                predictor = left
            elif method == 2:
                predictor = above
            elif method == 3:
                predictor = (left + above) // 2
            elif method == 4:
                estimate = left + above - upper_left
                distances = (abs(estimate - left), abs(estimate - above),
                             abs(estimate - upper_left))
                predictor = (left, above, upper_left)[distances.index(min(distances))]
            else:
                predictor = 0
            row[x] = (row[x] + predictor) & 0xFF
        rows.append(bytes(row))
        prior = row
    return width, height, rows


def load_json(path: Path) -> dict:
    require(path.is_file(), f"missing metadata: {path}")
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (UnicodeError, json.JSONDecodeError) as exc:
        raise AssetError(f"invalid JSON: {path}: {exc}") from exc
    require(isinstance(value, dict), f"expected JSON object: {path}")
    return value


def validate() -> None:
    manifest = load_json(MANIFEST)
    qa = load_json(QA)
    require((manifest.get("columns"), manifest.get("rows"),
             manifest.get("frame_width"), manifest.get("frame_height")) ==
            (COLUMNS, ROWS, FRAME_SIZE, FRAME_SIZE), "manifest grid/size mismatch")
    require(manifest.get("format") == "RGBA PNG" and
            manifest.get("sequence_order") == "row-major", "manifest format mismatch")
    frames = manifest.get("frames")
    require(isinstance(frames, list) and len(frames) == 24, "expected exactly 24 manifest frames")
    require([item.get("name") for item in frames if isinstance(item, dict)] ==
            list(EXPECTED_NAMES), "manifest frame names/order mismatch or duplicates")
    require((qa.get("canvas"), qa.get("frame_size"), qa.get("frame_count")) ==
            ([WIDTH, HEIGHT], [FRAME_SIZE, FRAME_SIZE], 24), "QA dimensions/count mismatch")
    require(qa.get("opaque_colors") == 48 and qa.get("alpha_values") == [0, 255],
            "QA palette/alpha metadata mismatch")

    atlas_width, atlas_height, atlas_rows = read_png(ATLAS)
    require((atlas_width, atlas_height) == (WIDTH, HEIGHT), "atlas dimensions mismatch")
    opaque_colors: set[bytes] = set()
    alpha_values: set[int] = set()
    for row in atlas_rows:
        for offset in range(0, len(row), 4):
            pixel = row[offset:offset + 4]
            alpha_values.add(pixel[3])
            if pixel[3] == 255:
                opaque_colors.add(pixel[:3])
    require(alpha_values == {0, 255}, f"atlas alpha values mismatch: {alpha_values}")
    require(len(opaque_colors) == 48,
            f"expected 48 opaque colors, found {len(opaque_colors)}")

    expected_files: set[str] = set()
    for index, item in enumerate(frames):
        require(isinstance(item, dict), f"invalid frame metadata at index {index}")
        row, column = divmod(index, COLUMNS)
        name = item["name"]
        require((item.get("row"), item.get("column")) == (row, column),
                f"frame grid position mismatch: {name}")
        x, y = column * FRAME_SIZE, row * FRAME_SIZE
        require(item.get("rect") == [x, y, FRAME_SIZE, FRAME_SIZE] and
                x + FRAME_SIZE <= WIDTH and y + FRAME_SIZE <= HEIGHT,
                f"frame rectangle invalid/outside atlas: {name}")
        provenance = "approved_master" if row < ROWS - 1 else "repaired_bottom_strip"
        require(item.get("source") == provenance, f"frame provenance mismatch: {name}")
        filename = f"r{row + 1:02d}-c{column + 1:02d}-{name}.png"
        expected_files.add(filename)
        frame_width, frame_height, frame_rows = read_png(CANDIDATE / "frames" / filename)
        require((frame_width, frame_height) == (FRAME_SIZE, FRAME_SIZE),
                f"frame dimensions mismatch: {filename}")
        for frame_y, frame_row in enumerate(frame_rows):
            atlas_row = atlas_rows[y + frame_y][x * 4:(x + FRAME_SIZE) * 4]
            require(frame_row == atlas_row,
                    f"frame pixels differ from atlas: {filename}, row {frame_y}")
    actual_files = {p.name for p in (CANDIDATE / "frames").glob("*.png")}
    require(actual_files == expected_files,
            f"frame file set mismatch: missing={sorted(expected_files - actual_files)}, "
            f"extra={sorted(actual_files - expected_files)}")
    require((ROOT / "assets" / "reference" / "original-character.png").is_file(),
            "missing original character reference")
    require((ROOT / "tools" / "build_sprite_atlas.py").is_file(),
            "missing original atlas builder")
    require(len(list((ROOT / "assets" / "source" / "generated").glob("*.png"))) == 2,
            "expected two generated source images")


if __name__ == "__main__":
    try:
        validate()
    except (AssetError, KeyError, TypeError, ValueError) as exc:
        print(f"Asset validation FAILED: {exc}", file=sys.stderr)
        raise SystemExit(1)
    print("Asset validation PASS: 24 frames, 512x768 atlas, exact pixels, palette and provenance")
