namespace Chudley.Core;

public readonly record struct PointD(double X, double Y);
public readonly record struct ScreenBounds(double Left, double Top, double Width, double Height)
{
    public double Right => Left + Width;
    public double Bottom => Top + Height;
}

public static class Positioning
{
    public static PointD Resolve(PetSettings settings, int petPixelSize, IReadOnlyList<ScreenBounds> screens)
    {
        if (petPixelSize <= 0) throw new ArgumentOutOfRangeException(nameof(petPixelSize));
        if (screens.Count == 0) throw new ArgumentException("At least one screen is required.", nameof(screens));
        var validScreens = screens.Where(s => s.Width > 0 && s.Height > 0).ToArray();
        if (validScreens.Length == 0) throw new ArgumentException("No screen has positive dimensions.", nameof(screens));
        var saved = settings.Validated();
        var target = validScreens[0];
        if (saved.Left is not null && saved.Top is not null)
        {
            var matching = validScreens
                .Select(screen => (screen, area: IntersectionArea(saved.Left.Value, saved.Top.Value, petPixelSize, screen)))
                .OrderByDescending(match => match.area).First();
            if (matching.area > 0) target = matching.screen;
        }
        else
        {
            return DefaultPoint(target, petPixelSize);
        }
        if (IntersectionArea(saved.Left!.Value, saved.Top!.Value, petPixelSize, target) == 0)
            return DefaultPoint(target, petPixelSize);
        return new PointD(
            Math.Clamp(saved.Left.Value, target.Left, Math.Max(target.Left, target.Right - petPixelSize)),
            Math.Clamp(saved.Top.Value, target.Top, Math.Max(target.Top, target.Bottom - petPixelSize)));
    }

    private static double IntersectionArea(double x, double y, int size, ScreenBounds screen) =>
        Math.Max(0, Math.Min(x + size, screen.Right) - Math.Max(x, screen.Left)) *
        Math.Max(0, Math.Min(y + size, screen.Bottom) - Math.Max(y, screen.Top));

    private static PointD DefaultPoint(ScreenBounds screen, int size) => new(
        Math.Max(screen.Left, screen.Right - size - 24),
        Math.Max(screen.Top, screen.Bottom - size - 24));
}
