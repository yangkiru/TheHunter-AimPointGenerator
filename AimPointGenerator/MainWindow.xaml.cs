using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AimPointGenerator.Controls;
using AimPointGenerator.Models;
using AimPointGenerator.Services;
using Microsoft.Win32;

namespace AimPointGenerator;

public partial class MainWindow
{
    private string? _lastLoadedPath;
    private BitmapSource? _lastImage;
    private AimPointItemControl? _draggedControl;

    public MainWindow()
    {
        InitializeComponent();

        AmmunitionNameBox.TextChanged += (_, _) => RefreshImage();
        EffectiveRangeBox.TextChanged += (_, _) => RefreshImage();
        ScopeNameBox.TextChanged += (_, _) => RefreshImage();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TryLoadLastSaved();
    }

    private void TryLoadLastSaved()
    {
        var path = DataStorage.GetLastFilePath();
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
        {
            LoadDefaultData();
            AddAimPointWithDefault();
            RefreshImage();
            return;
        }

        var data = DataStorage.Load(path);
        if (data != null)
        {
            _lastLoadedPath = path;
            ApplyData(data);
            StatusText.Text = $"Loaded last saved: {Path.GetFileName(path)}";
        }
        else
        {
            LoadDefaultData();
            AddAimPointWithDefault();
            RefreshImage();
        }
    }

    private void AddAimPointWithDefault()
    {
        var item = new AimPointItem
        {
            TargetDistance = 150,
            ZeroingDistance = 150,
            AimPosition = 0,
            IsMinZoom = false
        };
        var ctrl = CreateAimPointControl(item);
        AimPointItemsPanel.Children.Add(ctrl);
    }

    private void LoadDefaultData()
    {
        AmmunitionNameBox.Text = ".308";
        EffectiveRangeBox.Text = "150";
        ScopeNameBox.Text = "4-8x";
    }

    private AimPointData GetCurrentData()
    {
        _ = double.TryParse(EffectiveRangeBox.Text, out var range);
        var data = new AimPointData
        {
            AmmunitionName = AmmunitionNameBox.Text.Trim(),
            EffectiveRange = range,
            ScopeName = ScopeNameBox.Text.Trim(),
            AimPointItems = new List<AimPointItem>()
        };

        foreach (var child in AimPointItemsPanel.Children)
        {
            if (child is AimPointItemControl ctrl)
            {
                ctrl.SyncToItem();
                data.AimPointItems.Add(ctrl.Item);
            }
        }

        return data;
    }

    private void RefreshImage()
    {
        var data = GetCurrentData();
        _lastImage = AimPointImageGenerator.Generate(data);
        OutputImage.Source = _lastImage;
    }

    private void AddAimPoint_Click(object sender, RoutedEventArgs e)
    {
        _ = double.TryParse(EffectiveRangeBox.Text, out var effRange);
        var range = effRange > 0 ? effRange : 150;
        var item = new AimPointItem
        {
            TargetDistance = range,
            ZeroingDistance = range,
            AimPosition = 0,
            IsMinZoom = false
        };

        var ctrl = CreateAimPointControl(item);
        AimPointItemsPanel.Children.Add(ctrl);
        RefreshImage();
    }

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        var data = GetCurrentData();
        _lastImage = AimPointImageGenerator.Generate(data);

        var defaultPath = Path.Combine(DataStorage.GetDataFolder(), $"{data.GetFileName()}.png");
        var dialog = new SaveFileDialog
        {
            Filter = "PNG Image|*.png|All Files|*.*",
            DefaultExt = "png",
            FileName = Path.GetFileName(defaultPath),
            InitialDirectory = DataStorage.GetDataFolder()
        };

        if (dialog.ShowDialog() == true)
        {
            AimPointImageGenerator.SaveToFile(_lastImage, dialog.FileName);
            StatusText.Text = $"Saved: {dialog.FileName}";
        }
    }

    private void LoadData_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON File|*.json|All Files|*.*",
            DefaultExt = "json",
            InitialDirectory = DataStorage.GetDataFolder()
        };

        if (dialog.ShowDialog() != true) return;

        var data = DataStorage.Load(dialog.FileName);
        if (data == null)
        {
            StatusText.Text = "Unable to load file.";
            return;
        }

        _lastLoadedPath = dialog.FileName;
        ApplyData(data);
        StatusText.Text = $"Loaded: {Path.GetFileName(dialog.FileName)}";
    }

    private void SaveData_Click(object sender, RoutedEventArgs e)
    {
        var data = GetCurrentData();
        var savePath = _lastLoadedPath ?? DataStorage.GetDefaultFilePath(data);

        if (string.IsNullOrEmpty(_lastLoadedPath))
        {
            SaveDataAs_Click(sender, e);
            return;
        }

        var result = MessageBox.Show(
            $"Overwrite the current file?\n\n{savePath}",
            "Save Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        DataStorage.Save(data, savePath);
        StatusText.Text = $"Saved: {Path.GetFileName(savePath)}";
    }

    private void SaveDataAs_Click(object sender, RoutedEventArgs e)
    {
        var data = GetCurrentData();
        var defaultPath = DataStorage.GetDefaultFilePath(data);

        var dialog = new SaveFileDialog
        {
            Filter = "JSON File|*.json|All Files|*.*",
            DefaultExt = "json",
            FileName = Path.GetFileName(defaultPath),
            InitialDirectory = DataStorage.GetDataFolder()
        };

        if (dialog.ShowDialog() != true) return;

        DataStorage.Save(data, dialog.FileName);
        _lastLoadedPath = dialog.FileName;
        StatusText.Text = $"Saved: {Path.GetFileName(dialog.FileName)}";
    }

    private void ApplyData(AimPointData data)
    {
        AmmunitionNameBox.Text = data.AmmunitionName;
        EffectiveRangeBox.Text = data.EffectiveRange.ToString();
        ScopeNameBox.Text = data.ScopeName;

        AimPointItemsPanel.Children.Clear();
        foreach (var item in data.AimPointItems)
        {
            AimPointItemsPanel.Children.Add(CreateAimPointControl(item));
        }

        if (data.AimPointItems.Count == 0)
            AddAimPointWithDefault();

        RefreshImage();
    }

    private AimPointItemControl CreateAimPointControl(AimPointItem item)
    {
        var ctrl = new AimPointItemControl(item);
        ctrl.ValueChanged += (_, _) => RefreshImage();
        ctrl.DeleteRequested += (s, _) =>
        {
            if (s is AimPointItemControl toRemove)
            {
                AimPointItemsPanel.Children.Remove(toRemove);
                RefreshImage();
            }
        };
        ctrl.DragReorderRequested += OnDragReorderRequested;
        return ctrl;
    }

    private void OnDragReorderRequested(object? sender, AimPointItemControl control)
    {
        if (_draggedControl != null) return;
        _draggedControl = control;
        Mouse.Capture(control, CaptureMode.SubTree);
        control.PreviewMouseMove += OnDragMouseMove;
        control.PreviewMouseLeftButtonUp += OnDragMouseUp;
    }

    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedControl == null) return;
        var panel = AimPointItemsPanel;
        var pt = e.GetPosition(panel);
        var hit = VisualTreeHelper.HitTest(panel, pt);
        AimPointItemControl? targetCtrl = null;
        for (var v = hit?.VisualHit as DependencyObject; v != null; v = VisualTreeHelper.GetParent(v))
        {
            if (v is AimPointItemControl c)
            {
                targetCtrl = c;
                break;
            }
        }
        if (targetCtrl != null && targetCtrl != _draggedControl)
        {
            var fromIndex = panel.Children.IndexOf(_draggedControl);
            var targetIndex = panel.Children.IndexOf(targetCtrl);
            if (fromIndex >= 0 && targetIndex >= 0 && fromIndex != targetIndex)
            {
                panel.Children.RemoveAt(fromIndex);
                panel.Children.Insert(targetIndex, _draggedControl);
            }
        }
    }

    private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggedControl == null) return;
        _draggedControl.PreviewMouseMove -= OnDragMouseMove;
        _draggedControl.PreviewMouseLeftButtonUp -= OnDragMouseUp;
        Mouse.Capture(null);
        _draggedControl = null;
        RefreshImage();
    }
}
