using System.Globalization;
using System.Windows.Media;

namespace AimPointGenerator.Services;

/// <summary>
/// HSV ↔ RGB 변환 및 색상 유틸
/// </summary>
public static class ColorHelper
{
    public static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        var rf = r / 255.0;
        var gf = g / 255.0;
        var bf = b / 255.0;
        var max = Math.Max(rf, Math.Max(gf, bf));
        var min = Math.Min(rf, Math.Min(gf, bf));
        v = max;
        var delta = max - min;
        s = max < 0.0001 ? 0 : delta / max;
        h = 0;
        if (delta >= 0.0001)
        {
            if (max >= rf - 0.0001 && max <= rf + 0.0001)
                h = 60 * (((gf - bf) / delta) % 6);
            else if (max >= gf - 0.0001 && max <= gf + 0.0001)
                h = 60 * ((bf - rf) / delta + 2);
            else
                h = 60 * ((rf - gf) / delta + 4);
            if (h < 0) h += 360;
        }
    }

    public static Color HsvToRgb(double h, double s, double v, byte a = 255)
    {
        h = ((h % 360) + 360) % 360;
        s = Math.Clamp(s, 0, 1);
        v = Math.Clamp(v, 0, 1);
        var c = v * s;
        var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        var m = v - c;
        double r1 = 0, g1 = 0, b1 = 0;
        if (h < 60) { r1 = c; g1 = x; b1 = 0; }
        else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
        else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
        else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
        else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }
        return Color.FromArgb(a, (byte)((r1 + m) * 255), (byte)((g1 + m) * 255), (byte)((b1 + m) * 255));
    }

    public static Color ParseHex(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return Colors.Black;
        hex = hex.TrimStart('#').Trim();
        if (hex.Length == 0) return Colors.Black;
        try
        {
            if (hex.Length == 8)
            {
                var a = byte.Parse(hex[0..2], NumberStyles.HexNumber);
                var r = byte.Parse(hex[2..4], NumberStyles.HexNumber);
                var g = byte.Parse(hex[4..6], NumberStyles.HexNumber);
                var b = byte.Parse(hex[6..8], NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
            if (hex.Length == 6)
            {
                var r = byte.Parse(hex[0..2], NumberStyles.HexNumber);
                var g = byte.Parse(hex[2..4], NumberStyles.HexNumber);
                var b = byte.Parse(hex[4..6], NumberStyles.HexNumber);
                return Color.FromRgb(r, g, b);
            }
        }
        catch (FormatException) { /* invalid hex */ }
        return Colors.Black;
    }

    public static string ToHex(Color c, bool includeAlpha = false)
    {
        if (includeAlpha)
            return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
