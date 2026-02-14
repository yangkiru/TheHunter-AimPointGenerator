using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimPointGenerator.Controls;

public partial class CombineImageItemControl : UserControl
{
    public event EventHandler? DeleteRequested;
    public event EventHandler<CombineImageItemControl>? DragReorderRequested;

    public string FilePath { get; }

    public CombineImageItemControl(string filePath)
    {
        InitializeComponent();
        FilePath = filePath;
        FileNameText.Text = System.IO.Path.GetFileName(filePath);
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
