using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using Chudley.Core;

namespace Chudley.Desktop;

internal sealed class SpriteCatalog
{
    private readonly Dictionary<string, BitmapSource> _frames = new(StringComparer.Ordinal);

    public SpriteCatalog(string assetsDirectory, IEnumerable<string> requiredNames)
    {
        string metadataPath = Path.Combine(assetsDirectory, "pet.json");
        string pngPath = Path.Combine(assetsDirectory, "spritesheet.png");
        string webpPath = Path.Combine(assetsDirectory, "spritesheet.webp");
        foreach (string path in new[] { metadataPath, pngPath, webpPath })
            if (!File.Exists(path)) throw new FileNotFoundException("Required Chudley asset is missing.", path);

        using JsonDocument metadata = JsonDocument.Parse(File.ReadAllText(metadataPath));
        if (metadata.RootElement.GetProperty("spriteVersionNumber").GetInt32() != 2 ||
            metadata.RootElement.GetProperty("spritesheetPath").GetString() != "spritesheet.webp")
            throw new InvalidDataException("Unsupported Chudley pet metadata.");

        var atlas = new BitmapImage();
        atlas.BeginInit();
        atlas.UriSource = new Uri(pngPath, UriKind.Absolute);
        atlas.CacheOption = BitmapCacheOption.OnLoad;
        atlas.EndInit();
        atlas.Freeze();
        if (atlas.PixelWidth != AnimationCatalog.Columns * AnimationCatalog.FrameWidth ||
            atlas.PixelHeight != AnimationCatalog.Rows * AnimationCatalog.FrameHeight)
            throw new InvalidDataException("Chudley spritesheet dimensions are invalid.");

        foreach (var (name, cell) in AnimationCatalog.Frames)
        {
            var frame = new CroppedBitmap(atlas, new Int32Rect(
                cell.Column * AnimationCatalog.FrameWidth, cell.Row * AnimationCatalog.FrameHeight,
                AnimationCatalog.FrameWidth, AnimationCatalog.FrameHeight));
            frame.Freeze();
            _frames.Add(name, frame);
        }
        foreach (string name in requiredNames)
            if (!_frames.ContainsKey(name)) throw new InvalidDataException($"Animation references unknown frame: {name}");
    }

    public BitmapSource Get(string name) => _frames.TryGetValue(name, out BitmapSource? frame)
        ? frame : throw new InvalidDataException($"Unknown Chudley frame: {name}");
}
