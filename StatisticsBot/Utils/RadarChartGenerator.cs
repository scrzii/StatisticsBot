using SkiaSharp;
using StatisticsBot.Extensions;
using Features = System.Collections.Generic.Dictionary<string, float>;

namespace StatisticsBot.Utils;

public class RadarChartGenerator
{
    private readonly Features _maxValues;
    private readonly Dictionary<SKColor, Features> _objects;
    private readonly List<string> _features;
    private readonly float _radius;
    private readonly SKPoint _center;
    private ImageBuilder _builder;
    private readonly float _startAngle = (float)(Math.PI / 6);
    private readonly SKFont _font = new SKFont(SKTypeface.FromFamilyName("Cascadia Mono",
        new SKFontStyle(1, 0, SKFontStyleSlant.Upright)), 14);
    private readonly HashSet<string> _withPoints;

    public RadarChartGenerator(float radius)
    {
        _maxValues = new();
        _objects = new();
        _features = new();
        _withPoints = new();
        _radius = radius;
        _center = new SKPoint(radius + 20, radius + 20);
    }

    public void SetMax(string feature, float value)
    {
        _maxValues[feature] = value;
    }

    public void AddObject(SKColor color, object obj)
    {
        _objects[color] = new();
        foreach (var feature in _features)
        {
            var property = obj.GetType().GetProperties().FirstOrDefault(x => x.Name.ToLower() == feature.ToLower());
            if (property == null)
            {
                continue;
            }

            var value = property.GetValue(obj);
            try
            {
                _objects[color][feature] = Convert.ToSingle(value);
            }
            catch (Exception ex)
            {
                continue;
            }
        }
    }

    public void RegisterFeature(string name, float? maxValue = null)
    {
        _features.Add(name);
        if (maxValue != null)
        {
            _maxValues[name] = (float) maxValue;
        }
    }

    public void RegisterFeatures(params string[] names)
    {
        names.ToList().ForEach(x => RegisterFeature(x));
    }

    public void SetWithPoints(string feature)
    {
        _withPoints.Add(feature);
    }

    public SKImage Render()
    {
        InitMaxValues();
        _builder = ImageBuilder.Create((int)_radius * 2 + 40, (int)_radius * 2 + 40);

        RenderBackground();
        RenderObject(_maxValues, new SKColor(100, 100, 100), true);

        foreach (var obj in _objects)
        {
            RenderObject(obj.Value, obj.Key, true);
        }

        foreach (var obj in _objects)
        {
            RenderObject(obj.Value, obj.Key, false);
        }

        DrawAxis();

        return _builder.Build();
    }

    private void InitMaxValues()
    {
        foreach (var feature in _features.Where(x => !_maxValues.ContainsKey(x)).ToList())
        {
            _maxValues[feature] = _objects.Values.Max(x => x[feature]);
        }
    }

    private void RenderObject(Features obj, SKColor color, bool fill)
    {
        var angle = _startAngle;
        var path = new List<SKPoint>();
        foreach (var feature in obj)
        {
            var len = feature.Value / _maxValues[feature.Key] * _radius;
            var vec = new SKPoint((float)Math.Cos(angle) * len, -(float)Math.Sin(angle) * len);
            path.Add(SKPoint.Add(_center, vec));
            angle += (float)(Math.PI * 2 / _maxValues.Count);
        }
        _builder = _builder
            .NewPaint(new()
            {
                Color = color.SetAlpha(fill ? (byte)50 : (byte)255),
                Style = fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            })
            .Canvas((c, p) => c.DrawPath(FromPoints(path), p));
    }

    private void DrawAxis()
    {
        var axisPoints = Enumerable.Range(0, _features.Count)
            .Select(x =>
            {
                var angle = (float)(Math.PI * 2 / _features.Count * x + _startAngle);
                var vector = new SKPoint((float)(Math.Cos(angle) * _radius), -(float)Math.Sin(angle) * _radius);
                return SKPoint.Add(_center, vector);
            })
            .ToList();

        _builder = _builder
            .NewPaint(new()
            {
                Color = new SKColor(255, 255, 255),
                StrokeWidth = 1,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            });
        for (var i = 0; i < _features.Count; i++)
        {
            var point = axisPoints[i];
            var vec = SKPoint.Normalize(SKPoint.Subtract(point, _center));
            var offset = 20;
            vec.X *= offset;
            vec.Y *= offset;

            var featureName = _features[i];
            var turnOver = _center.X - point.X > 1;
            var path = FromPoints(turnOver ? new() { _center, point } : new() { point, _center });
            _builder = _builder
                .Paint(p => p.Color = p.Color.SetAlpha(128))
                .Paint(p => p.TextAlign = turnOver ? SKTextAlign.Center : SKTextAlign.Center)
                .Canvas((c, p) => c.DrawPath(path, p))
                .Canvas((c, p) => c.DrawTextOnPath(featureName, path, new SKPoint(_radius / 4, -15f), false, _font, p))
                .Paint(p => p.Style = SKPaintStyle.StrokeAndFill)
                .Paint(p => p.Color = p.Color.SetAlpha(255))
                .Canvas((c, p) => c.DrawCircle(point, 2, p))
                .Paint(p => p.TextAlign = SKTextAlign.Center)
                .Canvas((c, p) => c.DrawText(_maxValues[featureName].ToString(), SKPoint.Add(point, vec), p));

            if (_withPoints.Contains(featureName))
            {
                for (var j = 0; j < _maxValues[featureName]; j++)
                {
                    var pointVec = SKPoint.Normalize(vec);
                    pointVec.X *= _radius / _maxValues[featureName] * j;
                    pointVec.Y *= _radius / _maxValues[featureName] * j;
                    _builder = _builder
                        .Canvas((c, p) => c.DrawCircle(SKPoint.Add(_center, pointVec), 2, p));
                }
            }
        }
    }

    private void RenderBackground()
    {
        _builder = _builder
            .NewPaint(new()
            {
                Color = new SKColor(60, 60, 60),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            .Canvas((c, p) => c.DrawCircle(_center, _radius, p))
            .Paint(p =>
            {
                p.Style = SKPaintStyle.Stroke;
                p.StrokeWidth = 2;
                p.Color = new SKColor(150, 150, 150);
            })
            .Canvas((c, p) => c.DrawCircle(_center, _radius, p));
    }

    private SKPath FromPoints(List<SKPoint> path)
    {
        var result = new SKPath();
        result.AddPoly(path.ToArray());
        return result;
    }
}