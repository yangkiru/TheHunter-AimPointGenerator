using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AimPointGenerator.Services;

/// <summary>
/// Combines multiple images into a single grid image.
/// Images are arranged in rows with the specified number of columns.
/// </summary>
public static class ImageGridCombiner
{
    /// <summary>
    /// Combines images into a grid. Each row has up to 'columns' images.
    /// Example: 9 images, 4 columns -> row1: 1,2,3,4 | row2: 5,6,7,8 | row3: 9
    /// </summary>
    public static BitmapSource Combine(IReadOnlyList<BitmapSource> images, int columns)
    {
        if (images.Count == 0)
            throw new ArgumentException("At least one image is required.", nameof(images));
        if (columns < 1)
            columns = 1;

        var cellWidth = images[0].PixelWidth;
        var cellHeight = images[0].PixelHeight;

        var rowCount = (int)Math.Ceiling((double)images.Count / columns);
        var totalWidth = columns * cellWidth;
        var totalHeight = rowCount * cellHeight;

        var drawingVisual = new DrawingVisual();
        using (var dc = drawingVisual.RenderOpen())
        {
            for (var i = 0; i < images.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var x = col * cellWidth;
                var y = row * cellHeight;

                var image = images[i];
                var rect = new Rect(x, y, cellWidth, cellHeight);
                dc.DrawImage(image, rect);
            }
        }

        var renderBitmap = new RenderTargetBitmap(totalWidth, totalHeight, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(drawingVisual);
        return renderBitmap;
    }
}
