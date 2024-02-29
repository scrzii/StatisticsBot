using SkiaSharp;
using System.Text;

namespace StatisticsBot.Extensions;

public static class SkiaSharpExtensions
{
    public static async Task SaveToFile(this SKImage image, string filename)
    {
        using var data = image.Encode(SKEncodedImageFormat.Png, 0);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        var result = await stream.ReadAllBytes();
        File.WriteAllBytes(filename, result);
    }
}
