using SkiaSharp;

namespace StatisticsBot.Utils;

public static class ColorUtils
{
    public static SKColor GenerateRandom(long? seed = null)
    {
        var random = seed == null ? new Random() : new Random((int)seed);
        return new SKColor((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
    }
}
