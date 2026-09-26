using System.Text.Json;

namespace Chudley.Core;

public sealed record PetSettings
{
    public double? Left { get; init; }
    public double? Top { get; init; }
    public int Scale { get; init; } = 2;
    public bool Paused { get; init; }
    public int BehaviorFrequencyMinutes { get; init; } = 4;

    public PetSettings Validated() => this with
    {
        Left = IsValidCoordinate(Left) ? Left : null,
        Top = IsValidCoordinate(Top) ? Top : null,
        Scale = Scale is >= 1 and <= 4 ? Scale : 2,
        BehaviorFrequencyMinutes = BehaviorFrequencyMinutes is >= 1 and <= 60 ? BehaviorFrequencyMinutes : 4
    };

    private static bool IsValidCoordinate(double? value) =>
        value is null || (double.IsFinite(value.Value) && Math.Abs(value.Value) <= 1_000_000);
}

public sealed class PetSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    public string FilePath { get; }

    public PetSettingsStore(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Chudley", "settings.json");
    }

    public PetSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new PetSettings();
            return (JsonSerializer.Deserialize<PetSettings>(File.ReadAllText(FilePath), JsonOptions) ?? new PetSettings()).Validated();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new PetSettings();
        }
    }

    public void Save(PetSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var directory = Path.GetDirectoryName(FilePath) ?? throw new ArgumentException("Settings path needs a directory.");
        Directory.CreateDirectory(directory);
        var temporaryPath = FilePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings.Validated(), JsonOptions));
            File.Move(temporaryPath, FilePath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
