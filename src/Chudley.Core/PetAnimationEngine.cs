namespace Chudley.Core;

public interface IRandomSource
{
    int Next(int exclusiveMaximum);
}

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random;
    public SystemRandomSource(int? seed = null) => _random = seed is null ? new Random() : new Random(seed.Value);
    public int Next(int exclusiveMaximum) => _random.Next(exclusiveMaximum);
}

/// <summary>A small event boundary; no external integration is active in the MVP.</summary>
public interface IPetEventSink
{
    void Handle(PetEvent petEvent, DateTimeOffset now);
}

/// <summary>Time advances only when Tick is called. The UI owns the low-frequency timer.</summary>
public sealed class PetAnimationEngine : IPetEventSink
{
    private static readonly string[] SpecialBehaviors = ["snack", "drink", "wave", "personality"];
    private readonly IRandomSource _random;
    private readonly int _behaviorFrequencyMinutes;
    private AnimationDefinition _animation;
    private int _index;
    private DateTimeOffset _nextFrameAt;
    private DateTimeOffset _nextBehaviorAt;
    private DateTimeOffset _pausedAt;
    private PetState _stateBeforePause;
    private bool _dragging;

    public PetAnimationEngine(DateTimeOffset now, IRandomSource? random = null, int behaviorFrequencyMinutes = 4)
    {
        if (behaviorFrequencyMinutes is < 1 or > 60)
            throw new ArgumentOutOfRangeException(nameof(behaviorFrequencyMinutes));
        _random = random ?? new SystemRandomSource();
        _behaviorFrequencyMinutes = behaviorFrequencyMinutes;
        _animation = AnimationCatalog.Definitions["idle"];
        _nextFrameAt = now + _animation.Steps[0].Duration;
        ScheduleBehavior(now);
    }

    public string CurrentFrameName => _animation.Steps[_index].FrameName;
    public PetState State { get; private set; } = PetState.Idle;
    public bool IsPaused => State == PetState.Paused;
    public DateTimeOffset NextDueAt => IsPaused ? DateTimeOffset.MaxValue :
        State == PetState.Idle && _nextBehaviorAt < _nextFrameAt ? _nextBehaviorAt : _nextFrameAt;

    public void Tick(DateTimeOffset now)
    {
        if (IsPaused) return;
        if (State == PetState.Idle && now >= _nextBehaviorAt && !_dragging)
        {
            Start(SpecialBehaviors[_random.Next(SpecialBehaviors.Length)], now);
            return;
        }
        if (now < _nextFrameAt) return;

        if (_index + 1 < _animation.Steps.Count)
        {
            _index++;
            _nextFrameAt = now + _animation.Steps[_index].Duration;
        }
        else if (_animation.Loops)
        {
            _index = 0;
            _nextFrameAt = now + _animation.Steps[0].Duration;
        }
        else
        {
            ReturnToIdle(now);
        }
    }

    public void Click(DateTimeOffset now)
    {
        if (!IsPaused && !_dragging) Start("click", now);
    }

    public void DragStarted(DateTimeOffset now)
    {
        _dragging = true;
        if (!IsPaused) Start("movement", now);
    }

    public void DragEnded(DateTimeOffset now)
    {
        if (!_dragging) return;
        _dragging = false;
        if (!IsPaused) Start("movement", now);
    }

    public void Pause(DateTimeOffset now)
    {
        if (IsPaused) return;
        _stateBeforePause = State;
        _pausedAt = now;
        State = PetState.Paused;
    }

    public void Resume(DateTimeOffset now)
    {
        if (!IsPaused) return;
        var elapsed = now - _pausedAt;
        if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
        _nextFrameAt += elapsed;
        _nextBehaviorAt += elapsed;
        State = _stateBeforePause;
        if (_dragging) // A missed mouse-up must not keep an interaction alive forever.
        {
            _dragging = false;
            Start("movement", now);
        }
    }

    public void Handle(PetEvent petEvent, DateTimeOffset now)
    {
        // Only attention is wired today. The other event values reserve a narrow future input boundary.
        if (petEvent == PetEvent.AttentionRequested) Click(now);
    }

    private void Start(string name, DateTimeOffset now)
    {
        _animation = AnimationCatalog.Definitions[name];
        _index = 0;
        State = _animation.State;
        _nextFrameAt = now + _animation.Steps[0].Duration;
    }

    private void ReturnToIdle(DateTimeOffset now)
    {
        Start("idle", now);
        ScheduleBehavior(now);
    }

    private void ScheduleBehavior(DateTimeOffset now)
    {
        // 75-125% of the configured interval, so normal idle holds still for minutes.
        var baseMilliseconds = _behaviorFrequencyMinutes * 60_000;
        var offset = _random.Next(baseMilliseconds / 2 + 1);
        _nextBehaviorAt = now + TimeSpan.FromMilliseconds(baseMilliseconds * 3 / 4 + offset);
    }
}
