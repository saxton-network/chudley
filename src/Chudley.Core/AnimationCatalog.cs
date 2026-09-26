namespace Chudley.Core;

public enum PetState { Idle, Interaction, SpecialIdle, Movement, Paused }
public enum PetEvent { CodexWorking, CodexCompleted, CodexError, AttentionRequested }
public sealed record AnimationStep(string FrameName, TimeSpan Duration);
public sealed record AnimationDefinition(string Name, PetState State, bool Loops, IReadOnlyList<AnimationStep> Steps);

/// <summary>Codex v2 atlas coordinates and timings, independent of the window.</summary>
public static class AnimationCatalog
{
    public const int Columns = 8;
    public const int Rows = 11;
    public const int FrameWidth = 192;
    public const int FrameHeight = 208;
    public static readonly IReadOnlyDictionary<string, (int Row, int Column)> Frames = BuildFrames();
    public static readonly IReadOnlySet<string> AllFrameNames = Frames.Keys.ToHashSet(StringComparer.Ordinal);
    public static readonly IReadOnlyDictionary<string, AnimationDefinition> Definitions = BuildDefinitions();

    private static IReadOnlyDictionary<string, (int Row, int Column)> BuildFrames()
    {
        var frames = new Dictionary<string, (int, int)>(StringComparer.Ordinal);
        int[] counts = [6, 8, 8, 4, 5, 8, 6, 6, 6, 8, 8];
        string[] names = ["idle", "running_right", "running_left", "wave", "jump", "failed", "waiting", "running", "review", "look_0", "look_1"];
        for (int row = 0; row < counts.Length; row++)
            for (int column = 0; column < counts[row]; column++)
                frames.Add($"{names[row]}_{column}", (row, column));
        frames.Add("neutral", (0, 6));
        return frames;
    }

    private static IReadOnlyDictionary<string, AnimationDefinition> BuildDefinitions()
    {
        static AnimationDefinition Sequence(string name, PetState state, bool loops, int count, int milliseconds)
            => new(name, state, loops, Enumerable.Range(0, count)
                .Select(i => new AnimationStep($"{name}_{i}", TimeSpan.FromMilliseconds(milliseconds))).ToArray());
        var definitions = new[]
        {
            Sequence("idle", PetState.Idle, true, 6, 600),
            Sequence("running_right", PetState.Movement, true, 8, 110),
            Sequence("running_left", PetState.Movement, true, 8, 110),
            Sequence("wave", PetState.Interaction, false, 4, 240),
            Sequence("jump", PetState.SpecialIdle, false, 5, 170),
            Sequence("failed", PetState.SpecialIdle, false, 8, 220),
            Sequence("waiting", PetState.SpecialIdle, false, 6, 350),
            Sequence("running", PetState.SpecialIdle, false, 6, 150),
            Sequence("review", PetState.SpecialIdle, false, 6, 260)
        };
        foreach (var definition in definitions)
            if (definition.Steps.Count == 0 || definition.Steps.Any(s => s.Duration <= TimeSpan.Zero || !AllFrameNames.Contains(s.FrameName)))
                throw new InvalidOperationException($"Invalid animation definition: {definition.Name}");
        return definitions.ToDictionary(d => d.Name, StringComparer.Ordinal);
    }
}
