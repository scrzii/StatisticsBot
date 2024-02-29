using SkiaSharp;

namespace StatisticsBot.Utils;

public class ImageBuilder
{
    private readonly int _width = 0;
    private readonly int _height = 0;
    private readonly List<object> _pipeline;

    private ImageBuilder(int width, int height)
    {
        _width = width;
        _height = height;
        
        _pipeline = new();
    }

    public static ImageBuilder Create(int width, int height)
    {
        return new ImageBuilder(width, height);
    }

    public SKImage Build()
    {
        var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(new SKRect(0, 0, _width, _height));
        var paint = new SKPaint();

        foreach (var action in _pipeline)
        {
            var type = action.GetType();
            if (type == typeof(SKPaint))
            {
                paint = (SKPaint)action;
            }
            else if (type == typeof(Action<SKPaint>))
            {
                ((Action<SKPaint>)action).Invoke(paint);
            }
            else if (type == typeof(Action<SKCanvas, SKPaint>))
            {
                ((Action<SKCanvas, SKPaint>)action).Invoke(canvas, paint);
            }
        }

        var picture = recorder.EndRecording();
        return SKImage.FromPicture(picture, new SKSizeI(_width, _height));
    }

    public ImageBuilder Canvas(Action<SKCanvas, SKPaint> action)
    {
        _pipeline.Add(action);
        return this;
    }

    public ImageBuilder Paint(Action<SKPaint> action)
    {
        _pipeline.Add(action);
        return this;
    }

    public ImageBuilder NewPaint(SKPaint paint)
    {
        _pipeline.Add(paint);
        return this;
    }
}
