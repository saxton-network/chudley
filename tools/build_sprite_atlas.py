"""Package the approved visual master into an exact-cell, indexed sprite atlas.

This is deterministic extraction, placement, and color reduction, not animation
inbetweening. Keep the supplied visual references alongside the output.
"""

from pathlib import Path
import json
import zipfile
import shutil

import numpy as np
from PIL import Image


ROOT = Path(__file__).parent
SOURCE = ROOT / "generated_images/exec-4947ea28-beb5-41a7-baad-fcd31f5c7cf1.png"
BOTTOM = ROOT / "generated_images/exec-408ab73a-5fc8-4c1a-93da-ab850406e697.png"
OUT = ROOT / "chudley-production-candidate"
FRAME = 128
ROWS = 6
COLS = 4
ROW_EDGES = [0, 240, 478, 711, 941, 1163, 1366]
SOURCE_EDGES = [0, 288, 576, 864, 1151]

STATES = [
    ["front", "three_quarter_right", "three_quarter_left", "side_right"],
    ["reach_pocket", "grab_cheesy_puffs", "lift_cheesy_puffs", "eat_cheesy_puffs"],
    ["hold_red_can", "open_red_can_half_blink", "sip_red_can_closed_eyes", "lower_red_can_reopen"],
    ["rest", "move", "startle_move", "settle"],
    ["attention", "click", "shocked", "wave"],
    ["point", "smug_blue_lips", "amused_blue_lips", "signature_blue_lips"],
]


def bbox_for_alpha(im):
    alpha = np.asarray(im.getchannel("A"))
    ys, xs = np.where(alpha >= 128)
    if not len(xs):
        raise ValueError("empty frame")
    return int(xs.min()), int(ys.min()), int(xs.max() + 1), int(ys.max() + 1)


def crop_bottom_cell(im, c):
    width = im.width
    cell = im.crop((c * width // COLS, 0, (c + 1) * width // COLS, im.height))
    # Ignore isolated bottom-edge generation noise without redrawing any art.
    a = np.asarray(cell.getchannel("A")) >= 128
    rows = a.sum(axis=1)
    useful = np.flatnonzero(rows >= 6)
    if not len(useful):
        raise ValueError("empty generated bottom cell")
    end = min(cell.height, int(useful[-1]) + 2)
    return cell.crop((0, 0, cell.width, end))


def main():
    OUT.mkdir(exist_ok=True)
    frames_dir = OUT / "frames"
    frames_dir.mkdir(exist_ok=True)
    source = Image.open(SOURCE).convert("RGBA")
    bottom = Image.open(BOTTOM).convert("RGBA")
    raw = []
    for row in range(ROWS):
        one_row = []
        for col in range(COLS):
            if row == 5:
                cell = crop_bottom_cell(bottom, col)
            else:
                cell = source.crop((SOURCE_EDGES[col], ROW_EDGES[row],
                                    SOURCE_EDGES[col + 1], ROW_EDGES[row + 1]))
            box = bbox_for_alpha(cell)
            one_row.append(cell.crop(box))
        raw.append(one_row)

    # Use one scale for the first five approved rows, and one for the separately
    # generated final row. Never enlarge an individual pose to fill a cell.
    scales = []
    for group in (raw[:5], raw[5:]):
        sizes = [(im.width, im.height) for row in group for im in row]
        scales.append(min(112 / max(w for w, h in sizes),
                          110 / max(h for w, h in sizes)))

    atlas = Image.new("RGBA", (FRAME * COLS, FRAME * ROWS), (0, 0, 0, 0))
    manifest = {"format": "RGBA PNG", "columns": COLS, "rows": ROWS,
                "frame_width": FRAME, "frame_height": FRAME,
                "sequence_order": "row-major", "frames": []}
    for row in range(ROWS):
        for col in range(COLS):
            crop = raw[row][col]
            scale = scales[1 if row == 5 else 0]
            size = (max(1, round(crop.width * scale)),
                    max(1, round(crop.height * scale)))
            res = crop.resize(size, Image.Resampling.NEAREST)
            x = col * FRAME + (FRAME - res.width) // 2
            # Common baseline, with at least six transparent pixels below.
            y = row * FRAME + FRAME - 8 - res.height
            if x < col * FRAME + 4 or x + res.width > (col + 1) * FRAME - 4 or y < row * FRAME + 4:
                raise ValueError(f"frame {row},{col} exceeds padding")
            atlas.alpha_composite(res, (x, y))
            manifest["frames"].append({"row": row, "column": col,
                "name": STATES[row][col], "rect": [col * FRAME, row * FRAME, FRAME, FRAME],
                "source": "repaired_bottom_strip" if row == 5 else "approved_master"})

    # Exact binary alpha and a global shared opaque palette. Dithering is
    # disabled so no stray colors or semitransparent fringe enter the atlas.
    a = np.asarray(atlas.getchannel("A"))
    rgb = atlas.convert("RGB").quantize(colors=48, method=Image.Quantize.MEDIANCUT,
                                         dither=Image.Dither.NONE).convert("RGB")
    rgb.putalpha(Image.fromarray(np.where(a >= 128, 255, 0).astype("uint8"), "L"))
    atlas = rgb
    atlas_path = OUT / "chudley-atlas-4x6.png"
    atlas.save(atlas_path, optimize=True)
    for frame in manifest["frames"]:
        x, y, w, h = frame["rect"]
        atlas.crop((x, y, x + w, y + h)).save(frames_dir / f"r{frame['row']+1:02d}-c{frame['column']+1:02d}-{frame['name']}.png")

    qa = {"canvas": list(atlas.size), "frame_size": [FRAME, FRAME],
          "frame_count": len(manifest["frames"]),
          "opaque_colors": len(set(tuple(v) for v in np.asarray(atlas)[:,:,:3]
                                   [np.asarray(atlas)[:,:,3] == 255])),
          "alpha_values": sorted(set(np.asarray(atlas.getchannel("A")).ravel().tolist())),
          "cell_padding": [], "limitations": [
              "Generated poses are keyframes; this is not a complete runtime animation library.",
              "Face, scooter, hand, and can continuity still require human pixel-art QA.",
              "The last row was regenerated to recover wheels clipped in the approved master.",
          ]}
    for frame in manifest["frames"]:
        x, y, w, h = frame["rect"]
        cell = atlas.crop((x, y, x+w, y+h))
        box = bbox_for_alpha(cell)
        margins = [box[0], box[1], w-box[2], h-box[3]]
        qa["cell_padding"].append({"name": frame["name"], "margins_ltrb": margins})
        if min(margins) < 4:
            raise ValueError(f"clipped or near-clipped {frame['name']}: {margins}")
    (OUT / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    (OUT / "qa.json").write_text(json.dumps(qa, indent=2) + "\n")
    (OUT / "README.md").write_text(
        "# Chudley sprite atlas candidate\n\n"
        "24 transparent keyframes on an exact 4 × 6 grid. Each cell is 128 × 128 pixels; "
        "row-major coordinates and names are in `manifest.json`. Individual frame PNGs are in `frames/`. "
        "Colors use one shared 48-color palette and alpha is binary.\n\n"
        "The approved master defines the design. Its last row was cut off, so that row was regenerated "
        "before assembly. Read `qa.json` for structural checks and remaining visual work. "
        "The frames need artist review and inbetweens before treating this as a finished animation library.\n"
    )
    (OUT / "generated_images").mkdir(exist_ok=True)
    shutil.copy2(SOURCE, OUT / "generated_images" / SOURCE.name)
    shutil.copy2(BOTTOM, OUT / "generated_images" / BOTTOM.name)
    shutil.copy2(Path(__file__), OUT / Path(__file__).name)
    (OUT / "reference").mkdir(exist_ok=True)
    shutil.copy2(ROOT / "upload/01-1000007811.png", OUT / "reference/original-character.png")
    zip_path = ROOT / "chudley-production-candidate.zip"
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as z:
        for f in sorted(OUT.rglob("*")):
            if f.is_file():
                z.write(f, f.relative_to(OUT))
    print(json.dumps({"atlas": str(atlas_path), "zip": str(zip_path),
                      "qa": {k: qa[k] for k in ("canvas", "frame_size", "frame_count", "opaque_colors", "alpha_values")}}, indent=2))


if __name__ == "__main__":
    main()
