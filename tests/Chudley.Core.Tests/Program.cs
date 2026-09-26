using Chudley.Core;

int failures = 0;
Run("atlas definitions", AtlasDefinitions);
Run("deterministic idle progression", IdleProgression);
Run("click returns to idle", ClickReturnsToIdle);
Run("drag direction and settling", DragDirectionAndSettling);
Run("pause and resume", PauseAndResume);
Run("deterministic special selection", RandomBehavior);
Run("settings round trip and recovery", Settings);
Run("monitor position recovery", PositionRecovery);
Console.WriteLine(failures == 0 ? "All core tests passed." : $"{failures} core tests failed.");
return failures == 0 ? 0 : 1;

void Run(string name, Action test)
{
    try { test(); Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {name}: {ex}"); }
}

static void Eq<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected {expected}, got {actual}.");
}

static void True(bool condition)
{
    if (!condition) throw new Exception("Condition was false.");
}

static void AtlasDefinitions()
{
    Eq(1536, AnimationCatalog.Columns * AnimationCatalog.FrameWidth);
    Eq(2288, AnimationCatalog.Rows * AnimationCatalog.FrameHeight);
    foreach (var (name, cell) in AnimationCatalog.Frames)
    {
        True(name.IndexOfAny(['/', '\\']) < 0);
        True(cell.Row >= 0 && cell.Row < AnimationCatalog.Rows);
        True(cell.Column >= 0 && cell.Column < AnimationCatalog.Columns);
    }
    foreach (var definition in AnimationCatalog.Definitions.Values)
        foreach (var step in definition.Steps)
        {
            True(AnimationCatalog.AllFrameNames.Contains(step.FrameName));
            True(step.Duration > TimeSpan.Zero);
        }
    True(AnimationCatalog.Definitions["idle"].Loops);
    True(AnimationCatalog.Definitions["running_left"].Loops);
    True(!AnimationCatalog.Definitions["wave"].Loops);
}

static void IdleProgression()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    Eq("idle_0", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(599));
    Eq("idle_0", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(600));
    Eq("idle_1", engine.CurrentFrameName);
    for (int i = 2; i <= 6; i++) engine.Tick(t.AddMilliseconds(i * 600));
    Eq("idle_0", engine.CurrentFrameName);
}

static void ClickReturnsToIdle()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    engine.Click(t);
    Eq(PetState.Interaction, engine.State);
    for (int i = 1; i <= 4; i++) engine.Tick(t.AddMilliseconds(i * 240));
    Eq(PetState.Idle, engine.State);
    Eq("idle_0", engine.CurrentFrameName);
}

static void DragDirectionAndSettling()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    engine.DragStarted(t);
    Eq("running_right_0", engine.CurrentFrameName);
    engine.DragDirection(false, t.AddMilliseconds(50));
    Eq("running_left_0", engine.CurrentFrameName);
    engine.Click(t.AddMilliseconds(60));
    Eq(PetState.Movement, engine.State);
    engine.DragEnded(t.AddMilliseconds(70));
    Eq("waiting_0", engine.CurrentFrameName);
    for (int i = 1; i <= 6; i++) engine.Tick(t.AddMilliseconds(70 + i * 350));
    Eq(PetState.Idle, engine.State);
}

static void PauseAndResume()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    engine.Click(t);
    engine.Pause(t.AddMilliseconds(100));
    Eq(DateTimeOffset.MaxValue, engine.NextDueAt);
    engine.Tick(t.AddHours(1));
    Eq("wave_0", engine.CurrentFrameName);
    engine.Resume(t.AddHours(1));
    engine.Tick(t.AddHours(1).AddMilliseconds(139));
    Eq("wave_0", engine.CurrentFrameName);
    engine.Tick(t.AddHours(1).AddMilliseconds(140));
    Eq("wave_1", engine.CurrentFrameName);
}

static void RandomBehavior()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(1), 1);
    engine.Tick(t.AddSeconds(46));
    Eq("jump_0", engine.CurrentFrameName);
    Eq(PetState.SpecialIdle, engine.State);
}

static void Settings()
{
    var path = Path.Combine(Path.GetTempPath(), "chudley-tests-" + Guid.NewGuid(), "settings.json");
    try
    {
        var store = new PetSettingsStore(path);
        Eq(new PetSettings(), store.Load());
        var settings = new PetSettings { Left = -1280, Top = 50, Scale = 4, Paused = true };
        store.Save(settings);
        Eq(settings, store.Load());
        File.WriteAllText(path, "{not json");
        Eq(new PetSettings(), store.Load());
    }
    finally { if (File.Exists(path)) File.Delete(path); }
}

static void PositionRecovery()
{
    ScreenBounds[] screens = [new(0, 0, 1920, 1080), new(-1280, 0, 1280, 1024)];
    Eq(new PointD(-800, 300), Positioning.Resolve(new PetSettings { Left = -800, Top = 300 }, 384, 416, screens));
    PointD recovered = Positioning.Resolve(new PetSettings { Left = 5000, Top = 5000 }, 384, 416, screens);
    True(recovered.X >= 0 && recovered.X + 384 <= 1920 && recovered.Y >= 0 && recovered.Y + 416 <= 1080);
}

sealed class FixedRandom(int value) : IRandomSource
{
    public int Next(int exclusiveMaximum) => value % exclusiveMaximum;
}
