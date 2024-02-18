using SkiaSharp;

namespace StatisticsBot.Extensions;

public static class ColorExtensions
{
    public static SKColor SetAlpha(this SKColor color, byte alpha)
    {
        return new SKColor(color.Red, color.Green, color.Blue, alpha);
    }

    public static string ToHex(this SKColor color)
    {
        return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }
}
