using SkiaSharp;
using StatisticsBot.Extensions;
using StatisticsBot.Services.Data.Models;
using StatisticsBot.Utils;

namespace StatisticsBot.Services;

public class RenderService
{
    public const int Width = 725;
    public const int Height = 500;

    private const int ListX = 475;
    private const int ListY = 280;

    private static SKColor _background = new SKColor(80, 80, 80);
    private static SKFont _font = new SKFont(SKTypeface.FromFamilyName("Cambria",
        new SKFontStyle(1, 0, SKFontStyleSlant.Upright)), 14);


    public async Task<byte[]> Generate(List<User> users)
    {
        users = users.OrderByDescending(x => x.Rank).ThenByDescending(x => x.TotalTasks).ToList();

        var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(SKRect.Create(Width, Height));

        canvas.Clear(_background);
        canvas.Save();

        DrawUpdateInfo(canvas);
        DrawUsersData(canvas, users);
        DrawRadar(canvas, users);
        

        var picture = recorder.EndRecording();
        var image = SKImage.FromPicture(picture, new SKSizeI(Width, Height));
        using var data = image.Encode(SKEncodedImageFormat.Png, 0);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        var result = await stream.ReadAllBytes();

        if (Config.Instance.IsDev)
        {
            await SaveToFile("chart.png", result);
        }

        return result;
    }

    private void DrawUpdateInfo(SKCanvas canvas)
    {
        var image = ImageBuilder.Create(Width, Height)
            .Paint(p =>
            {
                p.Color = new SKColor(255, 255, 230);
                p.Style = SKPaintStyle.StrokeAndFill;
                p.TextAlign = SKTextAlign.Center;
                p.IsAntialias = true;
            })
            .Canvas((c, p) => c.DrawText($"Updated: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", Width / 2, 15, _font, p))
            .Build();

        canvas.DrawImage(image, SKPoint.Empty);
    }

    private void DrawUsersData(SKCanvas canvas, List<User> users)
    {
        var image = ImageBuilder.Create(Width, Height)
            .NewPaint(new SKPaint
            {
                Color = new SKColor(60, 60, 60),
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Left
            })
            .Canvas((c, p) => c.DrawRect(ListX - 10, ListY - 10, Width - ListX, Height - ListY, p))
            .Paint(p => p.Color = new SKColor(255, 255, 255))
            .Canvas((c, p) =>
            {
                for (var i = 0; i < users.Count; i++)
                {
                    var user = users[i];
                    var hColor = user.Color;

                    c.DrawRect(ListX, ListY + i * 24, 12, 12, new SKPaint() { Color = hColor, Style = SKPaintStyle.Fill });
                    c.DrawText($"{user.CodewarsLogin}: {user.Honor} ({user.TotalTasks})", ListX + 17, ListY + i * 24 + 10, _font, p);
                }
            })
            .Build();

        canvas.DrawImage(image, SKPoint.Empty);
    }

    private void DrawRadar(SKCanvas canvas, List<User> users)
    {
        var radar = new RadarChartGenerator(200);
        radar.RegisterFeatures("TotalTasks", "Honor", "Rank");
        radar.SetMax("Rank", 9);
        radar.SetWithPoints("Rank");
        foreach (var user in users)
        {
            radar.AddObject(user.Color, user);
        }
        canvas.DrawImage(radar.Render(), new SKPoint(10, 50));
    }

    public static async Task SaveToFile(string filename, byte[] data)
    {
        await File.WriteAllBytesAsync(filename, data);
    }
}
