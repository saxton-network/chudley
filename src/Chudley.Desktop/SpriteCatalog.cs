using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Chudley.Desktop;

// The manifest owns the logical-name to production-file mapping. The UI never
// embeds a frame filename, so an updated candidate can replace these PNGs.
internal sealed class SpriteCatalog
{
    private static readonly Regex SafeName = new("^[a-z0-9_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly Dictionary<string, BitmapSource> _frames;

    public SpriteCatalog(string assetsDirectory, IEnumerable<string> requiredNames)
    {
        string manifestPath = Path.Combine(assetsDirectory, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Required sprite manifest is missing", manifestPath);

        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        JsonElement root = manifest.RootElement;
        if (root.GetProperty("columns").GetInt32() != 4 ||
            root.GetProperty("rows").GetInt32() != 6 ||
            root.GetProperty("frame_width").GetInt32() != 128 ||
            root.GetProperty("frame_height").GetInt32() != 128)
            throw new InvalidDataException("Sprite manifest dimensions do not match the 4x6, 128px candidate.");

        JsonElement entries = root.GetProperty("frames");
        if (entries.GetArrayLength() != 24)
            throw new InvalidDataException("Sprite manifest must contain exactly 24 frames.");

        string framesDirectory = Path.Combine(assetsDirectory, "frames");
        _frames = new Dictionary<string, BitmapSource>(StringComparer.Ordinal);
        foreach (JsonElement entry in entries.EnumerateArray())
        {
            string name = entry.GetProperty("name").GetString() ?? "";
            int row = entry.GetProperty("row").GetInt32();
            int column = entry.GetProperty("column").GetInt32();
            if (!SafeName.IsMatch(name) || row is < 0 or > 5 || column is < 0 or > 3)
                throw new InvalidDataException($"Invalid sprite manifest entry: {name}");
            if (_frames.ContainsKey(name))
                throw new InvalidDataException($"Duplicate sprite name in manifest: {name}");

            string fileName = $"r{row + 1:00}-c{column + 1:00}-{name}.png";
            string path = Path.Combine(framesDirectory, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Required Chudley frame '{name}' is missing", path);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            if (bitmap.PixelWidth != 128 || bitmap.PixelHeight != 128)
                throw new InvalidDataException($"Frame '{name}' must be 128x128 pixels: {path}");
            _frames.Add(name, bitmap);
        }

        string[] missing = requiredNames.Where(name => !_frames.ContainsKey(name)).ToArray();
        if (missing.Length > 0)
            throw new InvalidDataException($"Animation references missing sprite frame(s): {string.Join(", ", missing)}");
    }

    public BitmapSource Get(string name) => _frames.TryGetValue(name, out BitmapSource? image)
        ? image
        : throw new InvalidDataException($"Unknown Chudley sprite frame: {name}");
}
