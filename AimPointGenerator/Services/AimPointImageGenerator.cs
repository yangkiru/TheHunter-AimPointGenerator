using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AimPointGenerator.Models;

namespace AimPointGenerator.Services;

/// <summary>
/// AimPoint 이미지 생성
/// Min/Max가 같은 점을 공유: 1 지점 왼쪽에 Min, 2 지점 오른쪽에 Max
/// </summary>
public static class AimPointImageGenerator
{
    private const int ImageSize = 500;
    private const int CenterX = 250;
    private const int CenterY = 250;
    private const int CircleRadius = 230;
    private static readonly double MaxPointExtent = CircleRadius * 4.0 / 5;
    private static readonly double ZoomLabelOffset = 18;
    private static readonly double ZoomLabelPush = 20;
    private static readonly Color ReticleColor = Color.FromRgb(0, 255, 100);
    private static readonly Color TextColor = Color.FromRgb(200, 255, 200);
    private static readonly Color BackgroundColor = Color.FromArgb(220, 0, 25, 0);

    public static BitmapSource Generate(AimPointData data)
    {
        var drawingVisual = new DrawingVisual();
        using (var dc = drawingVisual.RenderOpen())
        {
            var reticleBrush = new SolidColorBrush(ReticleColor);
            var textBrush = new SolidColorBrush(TextColor);
            var typeface = new Typeface("Segoe UI");

            dc.DrawRectangle(new SolidColorBrush(BackgroundColor), null, new Rect(0, 0, ImageSize, ImageSize));

            var circleGeometry = new EllipseGeometry(new Point(CenterX, CenterY), CircleRadius, CircleRadius);
            dc.DrawEllipse(null, new Pen(reticleBrush, 2), new Point(CenterX, CenterY), CircleRadius, CircleRadius);

            var sideMargin = 25;
            dc.DrawText(CreateText(data.AmmunitionName, typeface, 12, textBrush), new Point(sideMargin, 8));
            dc.DrawText(CreateText($"Eff. Range: {data.EffectiveRange}m", typeface, 10, textBrush), new Point(sideMargin, 26));

            var scopeText = CreateText(data.ScopeName, typeface, 12, textBrush);
            scopeText.TextAlignment = TextAlignment.Right;
            dc.DrawText(scopeText, new Point(ImageSize - sideMargin, 8));
            var hintText = CreateText("Target:Zeroing (e.g. 200:150)", typeface, 9, textBrush);
            hintText.TextAlignment = TextAlignment.Right;
            dc.DrawText(hintText, new Point(ImageSize - sideMargin, 26));

            dc.PushClip(circleGeometry);

            var circleLeft = CenterX - CircleRadius;
            var circleRight = CenterX + CircleRadius;
            var circleTop = CenterY - CircleRadius;
            var circleBottom = CenterY + CircleRadius;

            var pen = new Pen(reticleBrush, 1.5);
            dc.DrawLine(pen, new Point(circleLeft, CenterY), new Point(circleRight, CenterY));
            dc.DrawLine(pen, new Point(CenterX, circleTop), new Point(CenterX, circleBottom));

            var zoomLabelY = CenterY - ZoomLabelOffset;
            dc.DrawText(CreateText("Min Zoom", typeface, 9, textBrush), new Point(CenterX - MaxPointExtent - ZoomLabelPush + 5, zoomLabelY));
            var maxZoomText = CreateText("Max Zoom", typeface, 9, textBrush);
            maxZoomText.TextAlignment = TextAlignment.Right;
            dc.DrawText(maxZoomText, new Point(CenterX + MaxPointExtent + ZoomLabelPush - 5, zoomLabelY));

            DrawAimPointsShared(dc, data.AimPointItems, CenterX, CenterY, typeface, textBrush, reticleBrush);

            dc.Pop();
        }

        var renderBitmap = new RenderTargetBitmap(ImageSize, ImageSize, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(drawingVisual);
        return renderBitmap;
    }

    private static void DrawAimPointsShared(DrawingContext dc, List<AimPointItem> allItems,
        double centerX, double centerY, Typeface typeface, Brush textBrush, Brush reticleBrush)
    {
        var minItems = allItems.Where(x => x.IsMinZoom).ToList();
        var maxItems = allItems.Where(x => !x.IsMinZoom).ToList();
        var allPositions = allItems.Select(x => x.AimPosition).ToList();

        if (allPositions.Count == 0) return;

        var posMax = allPositions.Where(x => x > 0).DefaultIfEmpty(0).Max();
        var negMax = allPositions.Where(x => x < 0).Select(x => Math.Abs(x)).DefaultIfEmpty(0).Max();
        var posExtent = (int)Math.Ceiling(posMax);
        var negExtent = (int)Math.Ceiling(negMax);
        posExtent = Math.Max(posExtent, negExtent);
        negExtent = Math.Max(negExtent, posExtent);

        var minDataByPos = new Dictionary<int, AimPointItem>();
        var maxDataByPos = new Dictionary<int, AimPointItem>();
        foreach (var item in minItems)
        {
            if (Math.Abs(item.AimPosition - Math.Round(item.AimPosition)) < 0.001)
            {
                var pos = (int)Math.Round(item.AimPosition);
                if (pos != 0 && !minDataByPos.ContainsKey(pos)) minDataByPos[pos] = item;
            }
        }
        foreach (var item in maxItems)
        {
            if (Math.Abs(item.AimPosition - Math.Round(item.AimPosition)) < 0.001)
            {
                var pos = (int)Math.Round(item.AimPosition);
                if (pos != 0 && !maxDataByPos.ContainsKey(pos)) maxDataByPos[pos] = item;
            }
        }

        var extent = Math.Max(Math.Max(posExtent, negExtent), 1);
        var dotRadius = Math.Min(4.5, Math.Max(2.5, 4 - extent * 0.2));

        for (var pos = 1; pos <= posExtent; pos++)
        {
            var ratio = posExtent > 0 ? (double)pos / posExtent : 1;
            var offset = MaxPointExtent * ratio;
            var y = centerY + offset;

            dc.DrawEllipse(reticleBrush, null, new Point(centerX, y), dotRadius, dotRadius);

            if (minDataByPos.TryGetValue(pos, out var minItem) && (minItem.ZeroingDistance != 0 || minItem.TargetDistance != 0))
            {
                var label = $"{minItem.TargetDistance}:{minItem.ZeroingDistance}";
                var ft = CreateText(label, typeface, 8, textBrush);
                dc.DrawText(ft, new Point(centerX - dotRadius - 10 - ft.Width, y - 4));
            }
            if (maxDataByPos.TryGetValue(pos, out var maxItem) && (maxItem.ZeroingDistance != 0 || maxItem.TargetDistance != 0))
            {
                var label = $"{maxItem.TargetDistance}:{maxItem.ZeroingDistance}";
                dc.DrawText(CreateText(label, typeface, 8, textBrush), new Point(centerX + dotRadius + 10, y - 4));
            }
        }

        for (var pos = -1; pos >= -negExtent; pos--)
        {
            var ratio = negExtent > 0 ? (double)Math.Abs(pos) / negExtent : 1;
            var offset = MaxPointExtent * ratio;
            var y = centerY - offset;

            dc.DrawEllipse(reticleBrush, null, new Point(centerX, y), dotRadius, dotRadius);

            if (minDataByPos.TryGetValue(pos, out var minItem) && (minItem.ZeroingDistance != 0 || minItem.TargetDistance != 0))
            {
                var label = $"{minItem.TargetDistance}:{minItem.ZeroingDistance}";
                var ft = CreateText(label, typeface, 8, textBrush);
                dc.DrawText(ft, new Point(centerX - dotRadius - 10 - ft.Width, y - 4));
            }
            if (maxDataByPos.TryGetValue(pos, out var maxItem) && (maxItem.ZeroingDistance != 0 || maxItem.TargetDistance != 0))
            {
                var label = $"{maxItem.TargetDistance}:{maxItem.ZeroingDistance}";
                dc.DrawText(CreateText(label, typeface, 8, textBrush), new Point(centerX + dotRadius + 10, y - 4));
            }
        }

        foreach (var item in allItems.Where(x => Math.Abs(x.AimPosition - Math.Round(x.AimPosition)) >= 0.001 && Math.Abs(x.AimPosition) > 0.001))
        {
            var range = Math.Max(item.AimPosition > 0 ? posExtent : negExtent, 1);
            var ratio = Math.Abs(item.AimPosition) / range;
            var offset = MaxPointExtent * ratio * (item.AimPosition >= 0 ? 1 : -1);
            var y = centerY + offset;

            var lineLen = dotRadius * 1.2;
            dc.DrawLine(new Pen(reticleBrush, 1), new Point(centerX - lineLen, y), new Point(centerX + lineLen, y));
            var label = $"{item.TargetDistance}:{item.ZeroingDistance}";
            var ft = CreateText(label, typeface, 8, textBrush);
            if (item.IsMinZoom)
                dc.DrawText(ft, new Point(centerX - lineLen - 10 - ft.Width, y - 4));
            else
                dc.DrawText(ft, new Point(centerX + lineLen + 10, y - 4));
        }
    }

    private static FormattedText CreateText(string text, Typeface typeface, double fontSize, Brush brush)
    {
        return new FormattedText(text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface, fontSize, brush, 1.0);
    }

    public static void SaveToFile(BitmapSource bitmap, string filePath)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(filePath);
        encoder.Save(stream);
    }
}
