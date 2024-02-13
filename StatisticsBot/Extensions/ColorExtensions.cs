using SkiaSharp;

namespace StatisticsBot.Extensions;

public static class ColorExtensions
{
    public static SKColor ChangeAlpha(this SKColor color, byte alpha)
    {
        return new SKColor(color.Red, color.Green, color.Blue, alpha);
    }
}
