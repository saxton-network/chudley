namespace Chudley.Core;

public enum PetState { Idle, Interaction, SpecialIdle, Movement, Paused }

public enum PetEvent { CodexWorking, CodexCompleted, CodexError, AttentionRequested }

public sealed record AnimationStep(string FrameName, TimeSpan Duration);

public sealed record AnimationDefinition(
    string Name,
    PetState State,
    bool Loops,
    IReadOnlyList<AnimationStep> Steps);

/// <summary>Declarative sequences. Frame names are the logical names in manifest.json.</summary>
public static class AnimationCatalog
{
    public static readonly IReadOnlySet<string> AllFrameNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "front", "three_quarter_right", "three_quarter_left", "side_right",
        "reach_pocket", "grab_cheesy_puffs", "lift_cheesy_puffs", "eat_cheesy_puffs",
        "hold_red_can", "open_red_can_half_blink", "sip_red_can_closed_eyes", "lower_red_can_reopen",
        "rest", "move", "startle_move", "settle", "attention", "click", "shocked", "wave",
        "point", "smug_blue_lips", "amused_blue_lips", "signature_blue_lips"
    };

    public static readonly IReadOnlyDictionary<string, AnimationDefinition> Definitions = Build();

    private static IReadOnlyDictionary<string, AnimationDefinition> Build()
    {
        // Durations intentionally hold keyframes; the source bundle does not contain inbetweens.
        static AnimationStep Frame(string name, int milliseconds) => new(name, TimeSpan.FromMilliseconds(milliseconds));
        var definitions = new[]
        {
            new AnimationDefinition("idle", PetState.Idle, true, new[]
            {
                Frame("front", 2400), Frame("three_quarter_right", 1700),
                Frame("front", 2300), Frame("three_quarter_left", 1700), Frame("rest", 2200)
            }),
            new AnimationDefinition("snack", PetState.SpecialIdle, false, new[]
            {
                Frame("reach_pocket", 450), Frame("grab_cheesy_puffs", 550),
                Frame("lift_cheesy_puffs", 550), Frame("eat_cheesy_puffs", 1000), Frame("settle", 600)
            }),
            new AnimationDefinition("drink", PetState.SpecialIdle, false, new[]
            {
                Frame("hold_red_can", 550), Frame("open_red_can_half_blink", 500),
                Frame("sip_red_can_closed_eyes", 1050), Frame("lower_red_can_reopen", 600), Frame("settle", 600)
            }),
            new AnimationDefinition("click", PetState.Interaction, false, new[]
            {
                Frame("attention", 300), Frame("click", 300), Frame("shocked", 650), Frame("settle", 500)
            }),
            new AnimationDefinition("wave", PetState.SpecialIdle, false, new[]
            {
                Frame("attention", 350), Frame("wave", 850), Frame("settle", 500)
            }),
            new AnimationDefinition("personality", PetState.SpecialIdle, false, new[]
            {
                Frame("point", 650), Frame("smug_blue_lips", 650),
                Frame("amused_blue_lips", 700), Frame("signature_blue_lips", 950), Frame("settle", 600)
            }),
            new AnimationDefinition("movement", PetState.Movement, false, new[]
            {
                Frame("rest", 300), Frame("move", 400), Frame("startle_move", 450), Frame("settle", 600)
            })
        };
        foreach (var definition in definitions)
        {
            if (definition.Steps.Count == 0 || definition.Steps.Any(s => s.Duration <= TimeSpan.Zero || !AllFrameNames.Contains(s.FrameName)))
                throw new InvalidOperationException($"Invalid animation definition: {definition.Name}");
        }
        return definitions.ToDictionary(d => d.Name, StringComparer.Ordinal);
    }
}
