using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AimPointGenerator.Services;

namespace AimPointGenerator.Controls;

public partial class HueColorPicker : UserControl
{
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(HueColorPicker),
            new PropertyMetadata(Colors.Lime, OnSelectedColorChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(HueColorPicker),
            new PropertyMetadata("Color", (d, e) => { var p = (HueColorPicker)d; if (p.LabelText != null) p.LabelText.Text = (string)e.NewValue; }));

    public static readonly DependencyProperty IncludeAlphaProperty =
        DependencyProperty.Register(nameof(IncludeAlpha), typeof(bool), typeof(HueColorPicker),
            new PropertyMetadata(false, OnIncludeAlphaChanged));

    private static void OnIncludeAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HueColorPicker p && p.AlphaPanel != null)
            p.AlphaPanel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool _updating;

    public event EventHandler<Color>? ColorChanged;

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IncludeAlpha
    {
        get => (bool)GetValue(IncludeAlphaProperty);
        set => SetValue(IncludeAlphaProperty, value);
    }

    public HueColorPicker()
    {
        InitializeComponent();
        if (LabelText != null) LabelText.Text = Label;
        if (AlphaPanel != null) AlphaPanel.Visibility = IncludeAlpha ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HueColorPicker picker && !picker._updating)
            picker.SyncFromColor((Color)e.NewValue);
    }

    public void SetColorFromHex(string? hex)
    {
        if (HueSlider == null || SatSlider == null || ValSlider == null) return;
        _updating = true;
        try
        {
            SelectedColor = ColorHelper.ParseHex(hex);
            SyncFromColor(SelectedColor);
        }
        finally
        {
            _updating = false;
        }
    }

    private void SyncFromColor(Color c)
    {
        if (HueSlider == null || SatSlider == null || ValSlider == null) return;
        _updating = true;
        ColorHelper.RgbToHsv(c.R, c.G, c.B, out var h, out var s, out var v);
        HueSlider.Value = h;
        SatSlider.Value = s * 100;
        ValSlider.Value = v * 100;
        if (HueBox != null) HueBox.Text = ((int)h).ToString(CultureInfo.InvariantCulture);
        if (SatBox != null) SatBox.Text = ((int)(s * 100)).ToString(CultureInfo.InvariantCulture);
        if (ValBox != null) ValBox.Text = ((int)(v * 100)).ToString(CultureInfo.InvariantCulture);
        if (IncludeAlpha && AlphaSlider != null)
        {
            AlphaSlider.Value = c.A;
            if (AlphaBox != null) AlphaBox.Text = c.A.ToString(CultureInfo.InvariantCulture);
        }
        if (PreviewBorder != null) PreviewBorder.Background = new SolidColorBrush(c);
        if (HexText != null) HexText.Text = ColorHelper.ToHex(c, IncludeAlpha);
        _updating = false;
    }

    private void UpdateColor()
    {
        if (_updating) return;
        if (HueSlider == null || SatSlider == null || ValSlider == null) return;
        var h = HueSlider.Value;
        var s = SatSlider.Value / 100.0;
        var v = ValSlider.Value / 100.0;
        var a = (byte)(IncludeAlpha && AlphaSlider != null ? Math.Clamp((int)AlphaSlider.Value, 0, 255) : 255);
        var c = ColorHelper.HsvToRgb(h, s, v, a);
        _updating = true;
        SelectedColor = c;
        if (PreviewBorder != null) PreviewBorder.Background = new SolidColorBrush(c);
        if (HexText != null) HexText.Text = ColorHelper.ToHex(c, IncludeAlpha);
        _updating = false;
        ColorChanged?.Invoke(this, c);
    }

    private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updating) return;
        if (HueBox != null) HueBox.Text = ((int)HueSlider.Value).ToString(CultureInfo.InvariantCulture);
        UpdateColor();
    }

    private void SatSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updating) return;
        if (SatBox != null) SatBox.Text = ((int)SatSlider.Value).ToString(CultureInfo.InvariantCulture);
        UpdateColor();
    }

    private void ValSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updating) return;
        if (ValBox != null) ValBox.Text = ((int)ValSlider.Value).ToString(CultureInfo.InvariantCulture);
        UpdateColor();
    }

    private void HueBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updating) return;
        if (double.TryParse(HueBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
        {
            v = Math.Clamp(v, 0, 360);
            HueSlider.Value = v;
            UpdateColor();
        }
    }

    private void HueBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(HueBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            HueBox.Text = ((int)Math.Clamp(v, 0, 360)).ToString(CultureInfo.InvariantCulture);
        else
            HueBox.Text = ((int)HueSlider.Value).ToString(CultureInfo.InvariantCulture);
    }

    private void SatBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updating) return;
        if (double.TryParse(SatBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
        {
            v = Math.Clamp(v, 0, 100);
            SatSlider.Value = v;
            UpdateColor();
        }
    }

    private void SatBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(SatBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            SatBox.Text = ((int)Math.Clamp(v, 0, 100)).ToString(CultureInfo.InvariantCulture);
        else
            SatBox.Text = ((int)SatSlider.Value).ToString(CultureInfo.InvariantCulture);
    }

    private void ValBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updating) return;
        if (double.TryParse(ValBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
        {
            v = Math.Clamp(v, 0, 100);
            ValSlider.Value = v;
            UpdateColor();
        }
    }

    private void ValBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(ValBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            ValBox.Text = ((int)Math.Clamp(v, 0, 100)).ToString(CultureInfo.InvariantCulture);
        else
            ValBox.Text = ((int)ValSlider.Value).ToString(CultureInfo.InvariantCulture);
    }

    private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updating) return;
        if (AlphaBox != null) AlphaBox.Text = ((int)AlphaSlider.Value).ToString(CultureInfo.InvariantCulture);
        UpdateColor();
    }

    private void AlphaBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updating) return;
        if (int.TryParse(AlphaBox?.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            v = Math.Clamp(v, 0, 255);
            AlphaSlider.Value = v;
            UpdateColor();
        }
    }

    private void AlphaBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (AlphaBox == null) return;
        if (int.TryParse(AlphaBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
            AlphaBox.Text = Math.Clamp(v, 0, 255).ToString(CultureInfo.InvariantCulture);
        else if (AlphaSlider != null)
            AlphaBox.Text = ((int)AlphaSlider.Value).ToString(CultureInfo.InvariantCulture);
    }
}
