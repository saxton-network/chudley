using Chudley.Core;

var failures = 0;
Run("animation names and definitions", AnimationNames);
Run("deterministic idle frame progression", IdleProgression);
Run("click one-shot returns to idle", ClickReturnsToIdle);
Run("drag suppresses click and settles", DragSuppressesClick);
Run("pause and resume preserve remaining time", PauseAndResume);
Run("random special behavior is deterministic", RandomBehavior);
Run("settings defaults and serialization", SettingsRoundTrip);
Run("malformed settings recover safely", MalformedSettings);
Run("position remains visible across monitors", PositionRecovery);
Console.WriteLine(failures == 0 ? "All 9 core tests passed." : $"{failures} core tests failed.");
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

static void AnimationNames()
{
    Eq(24, AnimationCatalog.AllFrameNames.Count);
    foreach (var definition in AnimationCatalog.Definitions.Values)
    foreach (var step in definition.Steps)
    {
        True(AnimationCatalog.AllFrameNames.Contains(step.FrameName));
        True(step.FrameName.IndexOfAny(['/', '\\']) < 0);
        True(step.Duration > TimeSpan.Zero);
    }
    True(AnimationCatalog.Definitions["idle"].Loops);
    foreach (var definition in AnimationCatalog.Definitions.Values.Where(d => d.Name != "idle"))
        True(!definition.Loops);
}

static void IdleProgression()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    Eq("front", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(2399));
    Eq("front", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(2400));
    Eq("three_quarter_right", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(4100));
    Eq("front", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(6400));
    Eq("three_quarter_left", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(8100));
    Eq("rest", engine.CurrentFrameName);
    engine.Tick(t.AddMilliseconds(10300));
    Eq("front", engine.CurrentFrameName);
}

static void ClickReturnsToIdle()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    engine.Click(t);
    Eq(PetState.Interaction, engine.State);
    foreach (var step in AnimationCatalog.Definitions["click"].Steps)
    {
        Eq(step.FrameName, engine.CurrentFrameName);
        t += step.Duration;
        engine.Tick(t);
    }
    Eq(PetState.Idle, engine.State);
    Eq("front", engine.CurrentFrameName);
}

static void DragSuppressesClick()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    engine.DragStarted(t);
    Eq(PetState.Movement, engine.State);
    engine.Click(t.AddMilliseconds(50));
    Eq(PetState.Movement, engine.State);
    engine.DragEnded(t.AddMilliseconds(100));
    foreach (var step in AnimationCatalog.Definitions["movement"].Steps)
    {
        t += step.Duration + TimeSpan.FromMilliseconds(100);
        engine.Tick(t);
    }
    Eq(PetState.Idle, engine.State);
    engine.Click(t);
    Eq(PetState.Interaction, engine.State);
}

static void PauseAndResume()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(0));
    engine.Click(t);
    engine.Pause(t.AddMilliseconds(100));
    Eq(PetState.Paused, engine.State);
    Eq(DateTimeOffset.MaxValue, engine.NextDueAt);
    engine.Tick(t.AddHours(1));
    Eq("attention", engine.CurrentFrameName);
    engine.Resume(t.AddHours(1));
    Eq(PetState.Interaction, engine.State);
    engine.Tick(t.AddHours(1).AddMilliseconds(199));
    Eq("attention", engine.CurrentFrameName);
    engine.Tick(t.AddHours(1).AddMilliseconds(200));
    Eq("click", engine.CurrentFrameName);
}

static void RandomBehavior()
{
    var t = DateTimeOffset.UnixEpoch;
    var engine = new PetAnimationEngine(t, new FixedRandom(1), 1);
    // Fixed random gives the same scheduling and chooses index 1: drink.
    engine.Tick(t.AddSeconds(46));
    Eq(PetState.SpecialIdle, engine.State);
    Eq("hold_red_can", engine.CurrentFrameName);
    foreach (var step in AnimationCatalog.Definitions["drink"].Steps)
    {
        t += step.Duration + TimeSpan.FromSeconds(46);
        engine.Tick(t);
    }
    Eq(PetState.Idle, engine.State);
}

static void SettingsRoundTrip()
{
    var path = Path.Combine(Path.GetTempPath(), "chudley-core-tests-" + Guid.NewGuid(), "settings.json");
    try
    {
        var store = new PetSettingsStore(path);
        Eq(new PetSettings(), store.Load());
        var settings = new PetSettings { Left = -1280.5, Top = 50, Scale = 4, Paused = true, BehaviorFrequencyMinutes = 7 };
        store.Save(settings);
        Eq(settings, store.Load());
        True(File.ReadAllText(path).Contains("\"scale\": 4", StringComparison.Ordinal));
    }
    finally { if (File.Exists(path)) File.Delete(path); }
}

static void MalformedSettings()
{
    var path = Path.Combine(Path.GetTempPath(), "chudley-core-tests-" + Guid.NewGuid(), "settings.json");
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{not json");
        Eq(new PetSettings(), new PetSettingsStore(path).Load());
        File.WriteAllText(path, "{\"scale\":99,\"behaviorFrequencyMinutes\":0,\"left\":2000000}");
        var recovered = new PetSettingsStore(path).Load();
        Eq(2, recovered.Scale);
        Eq(4, recovered.BehaviorFrequencyMinutes);
        Eq<double?>(null, recovered.Left);
    }
    finally { if (File.Exists(path)) File.Delete(path); }
}

static void PositionRecovery()
{
    ScreenBounds[] screens = [new(0, 0, 1920, 1080), new(-1280, 0, 1280, 1024)];
    Eq(new PointD(-800, 300), Positioning.Resolve(new PetSettings { Left = -800, Top = 300 }, 256, screens));
    var recovered = Positioning.Resolve(new PetSettings { Left = 5000, Top = 5000 }, 256, screens);
    True(recovered.X >= 0 && recovered.X + 256 <= 1920 && recovered.Y >= 0 && recovered.Y + 256 <= 1080);
    var clamped = Positioning.Resolve(new PetSettings { Left = 1900, Top = 1000 }, 256, screens);
    Eq(new PointD(1664, 824), clamped);
    var fallback = Positioning.Resolve(new PetSettings(), 256, screens);
    True(fallback.X >= 0 && fallback.Y >= 0);
}

sealed class FixedRandom(int value) : IRandomSource
{
    public int Next(int exclusiveMaximum) => value % exclusiveMaximum;
}
