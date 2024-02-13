using SkiaSharp;
using StatisticsBot.Extensions;
using StatisticsBot.Sql.Models;

namespace StatisticsBot.Render;

public static class ChartGenerator
{
    private const int Width = 500;
    private const int Height = 400;
    private const int MaxLength = 190;
    private const int CenterX = 200;
    private const int CenterY = 150;
    private const int ListX = 340;
    private const int ListY = 280;

    private static SKColor _background = new SKColor(80, 80, 80);
    private static SKColor _background2 = new SKColor(100, 100, 100);
    private static SKFont _font = new SKFont(SKTypeface.FromFamilyName("Cascadia Mono", 
        new SKFontStyle(1, 0, SKFontStyleSlant.Upright)), 14);

    public static void Generate(List<User> users)
    {
        var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(SKRect.Create(Width, Height));

        canvas.Clear(_background);
        canvas.Save();
        

        FillChart(canvas, 1, 1, 1, _background2, true);

        var now = DateTime.Now;
        var paint = new SKPaint()
        {
            Color = new SKColor(255, 255, 230),
            Style = SKPaintStyle.Stroke,
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText($"Updated: {now:dd.MM.yyyy HH:mm:ss}", Width / 2, 15, _font, paint);
        paint.Color = new SKColor(60, 60, 60);
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawRect(ListX - 10, ListY - 10, Width - ListX, Height - ListY, paint);

        var honorMax = users.Max(x => x.Honor);
        var totalMax = users.Max(x => x.TotalTasks);
        var rankMax = 9;

        var index = 0;
        paint.Color = new SKColor(255, 255, 255);
        paint.TextAlign = SKTextAlign.Left;
        foreach (var user in users)
        {
            var hColor = SKColor.Parse(user.Color);
            var color = hColor.ChangeAlpha(50);

            var honor = (double)(user.Honor / (double)honorMax);
            var rank = (double)(user.Rank/ (double)rankMax);
            var total = (double)(user.TotalTasks / (double)totalMax);
            FillChart(canvas, honor, rank, total, color);

            canvas.DrawRect(ListX, ListY + index * 24, 12, 12, new SKPaint() { Color = hColor, Style = SKPaintStyle.Fill });
            canvas.DrawText(user.CodewarsLogin, ListX + 17, ListY + index * 24 + 10, _font, paint);

            index++;
        }

        var centerPaint = new SKPaint()
        {
            Color = new SKColor(255, 255, 255),
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(CenterX, CenterY, 2.5f, centerPaint);

        var picture = recorder.EndRecording();
        var image = SKImage.FromPicture(picture, new SKSizeI(Width, Height));
        using var data = image.Encode(SKEncodedImageFormat.Png, 0);
        using var imageStream = File.OpenWrite("chart.png");
        data.SaveTo(imageStream);
    }

    private static void FillChart(SKCanvas canvas, double honor, double rank, double total, SKColor color, bool text = false)
    {
        var paint = new SKPaint()
        {
            Color = color,
            Style = SKPaintStyle.Fill
        };
        var textPaint = new SKPaint()
        {
            Color = new SKColor(255, 255, 255),
            Style = SKPaintStyle.Stroke,
        };

        var totalVec = GenerateDirection(30);
        var honorVec = GenerateDirection(150);
        var rankVec = GenerateDirection(270);

        var totalPoint = GetPoint(totalVec, MaxLength * total);
        canvas.DrawCircle(totalPoint.X, totalPoint.Y, 2.5f, paint);

        var honorPoint = GetPoint(honorVec, MaxLength * honor);
        canvas.DrawCircle(honorPoint.X, honorPoint.Y, 2.5f, paint);

        var rankPoint = GetPoint(rankVec, MaxLength * rank);
        canvas.DrawCircle(rankPoint.X, rankPoint.Y, 2.5f, paint);

        FillPath(canvas, paint, new[] { totalPoint, honorPoint, rankPoint });
        if (text)
        {
            canvas.DrawText("Total tasks", totalPoint.X + 10, totalPoint.Y, _font, textPaint);
            canvas.DrawText("Honor", honorPoint.X, honorPoint.Y - 17, _font, textPaint);
            canvas.DrawText("Rank", rankPoint.X + 5, rankPoint.Y + 17, _font, textPaint);
        }
    }

    private static SKPoint GetPoint(SKPoint direction, double length)
    {
        return new SKPoint((int)(CenterX + direction.X * length), (int)(CenterY + direction.Y * length));
    }

    private static void FillPath(SKCanvas canvas, SKPaint paint, SKPoint[] points)
    {
        var path = new SKPath();
        path.AddPoly(points);
        canvas.DrawPath(path, paint);
    }

    private static SKPoint GenerateDirection(double deg)
    {
        var rad = Math.PI / 180 * deg;
        return new SKPoint((float)Math.Cos(rad), (float)-Math.Sin(rad));
    }
}
