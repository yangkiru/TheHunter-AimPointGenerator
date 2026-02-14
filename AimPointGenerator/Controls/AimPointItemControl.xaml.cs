using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using System.Text.RegularExpressions;
using AimPointGenerator.Models;

namespace AimPointGenerator.Controls;

public partial class AimPointItemControl : UserControl
{
    public event EventHandler? ValueChanged;
    public event EventHandler? DeleteRequested;
    public event EventHandler<AimPointItemControl>? DragReorderRequested;

    public AimPointItem Item { get; }

    public AimPointItemControl(AimPointItem item)
    {
        InitializeComponent();
        Item = item;

        TargetBox.Text = item.TargetDistance.ToString();
        ZeroingBox.Text = item.ZeroingDistance.ToString();
        Item.AimPosition = Math.Clamp(item.AimPosition, -10, 10);
        AimPositionBox.Text = Item.AimPosition.ToString();
        UpdateMinMaxButton();

        TargetBox.TextChanged += (_, _) => OnValueChanged();
        ZeroingBox.TextChanged += (_, _) => OnValueChanged();
        AimPositionBox.TextChanged += (_, _) => OnValueChanged();
    }

    public void SyncToItem()
    {
        if (TryParseNumericInput(TargetBox.Text, out var t, out var normalizedTarget))
        {
            Item.TargetDistance = t;
            if (!IsTypingDecimal(TargetBox.Text) && TargetBox.Text != normalizedTarget) TargetBox.Text = normalizedTarget;
        }

        if (TryParseNumericInput(ZeroingBox.Text, out var z, out var normalizedZeroing))
        {
            Item.ZeroingDistance = z;
            if (!IsTypingDecimal(ZeroingBox.Text) && ZeroingBox.Text != normalizedZeroing) ZeroingBox.Text = normalizedZeroing;
        }

        if (TryParseNumericInput(AimPositionBox.Text, out var a, out var normalizedAim))
        {
            Item.AimPosition = Math.Clamp(a, -10, 10);
            if (!IsTypingDecimal(AimPositionBox.Text))
            {
                if (AimPositionBox.Text != normalizedAim) AimPositionBox.Text = normalizedAim;
                if (Math.Abs(a - Item.AimPosition) > 0.001)
                    AimPositionBox.Text = FormatNumber(Item.AimPosition);
            }
        }
    }

    /// <summary>
    /// 입력이 "."으로 끝나면 아직 소수 입력 중 (2.5 입력 가능하도록 텍스트 교체 생략)
    /// </summary>
    private static bool IsTypingDecimal(string text)
    {
        return !string.IsNullOrEmpty(text) && text.TrimEnd().EndsWith(".");
    }

    private static bool TryParseNumericInput(string raw, out double value, out string normalized)
    {
        value = 0;
        normalized = raw;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var input = raw.Trim();
        if (TryEvaluateExpression(input, out value))
        {
            normalized = FormatNumber(value);
            return true;
        }

        return false;
    }

    private static bool TryEvaluateExpression(string input, out double value)
    {
        value = 0;

        // .5 / -.5 / +.5 형태를 0.5 계열로 보정
        input = Regex.Replace(input, @"(?<=^|[+\-*/\s])([+-]?)\.(\d+)", "${1}0.$2");

        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;
        if (double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return true;

        var match = Regex.Match(input, @"^\s*([+-]?(?:\d+(?:\.\d+)?|\.\d+))\s*([+\-*/])\s*([+-]?(?:\d+(?:\.\d+)?|\.\d+))\s*$");
        if (!match.Success) return false;

        if (!double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var left))
            return false;
        if (!double.TryParse(match.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var right))
            return false;

        switch (match.Groups[2].Value)
        {
            case "+": value = left + right; return true;
            case "-": value = left - right; return true;
            case "*": value = left * right; return true;
            case "/":
                if (Math.Abs(right) < 0.0000001) return false;
                value = left / right;
                return true;
            default:
                return false;
        }
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###############", CultureInfo.InvariantCulture);
    }

    private void MinMaxButton_Click(object sender, RoutedEventArgs e)
    {
        Item.IsMinZoom = !Item.IsMinZoom;
        UpdateMinMaxButton();
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateMinMaxButton()
    {
        MinMaxButton.Content = Item.IsMinZoom ? "Min" : "Max";
        MinMaxButton.Background = Item.IsMinZoom
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3d, 0x6b, 0x9e))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2d, 0x5a, 0x2d));
    }

    private void OnValueChanged()
    {
        SyncToItem();
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void NumericBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBox box) return;
        box.Focus();
        box.SelectAll();
        e.Handled = true;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        DragReorderRequested?.Invoke(this, this);
    }
}
