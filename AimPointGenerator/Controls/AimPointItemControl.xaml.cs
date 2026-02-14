using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        if (double.TryParse(TargetBox.Text, out var t)) Item.TargetDistance = t;
        if (double.TryParse(ZeroingBox.Text, out var z)) Item.ZeroingDistance = z;
        if (double.TryParse(AimPositionBox.Text, out var a))
        {
            Item.AimPosition = Math.Clamp(a, -10, 10);
            if (Math.Abs(a - Item.AimPosition) > 0.001)
                AimPositionBox.Text = Item.AimPosition.ToString();
        }
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
