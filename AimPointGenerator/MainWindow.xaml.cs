using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;
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
    private CombineImageItemControl? _draggedCombineControl;

    public MainWindow()
    {
        InitializeComponent();

        AmmunitionNameBox.TextChanged += (_, _) => RefreshImage();
        EffectiveRangeBox.TextChanged += (_, _) => OnEffectiveRangeChanged();
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
        var range = 0.0;
        if (TryParseNumericInput(EffectiveRangeBox.Text, out var parsedRange, out var normalizedRange))
        {
            range = parsedRange;
            if (EffectiveRangeBox.Text != normalizedRange) EffectiveRangeBox.Text = normalizedRange;
        }
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
        _ = TryParseNumericInput(EffectiveRangeBox.Text, out var effRange, out _);
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

    private void OpenDirectory_Click(object sender, RoutedEventArgs e)
    {
        var folder = DataStorage.GetDataFolder();
        if (Directory.Exists(folder))
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
            StatusText.Text = $"Opened: {folder}";
        }
        else
        {
            StatusText.Text = "Data folder does not exist.";
        }
    }

    private void SelectCombineImages_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "PNG Image|*.png|All Files|*.*",
            Multiselect = true,
            Title = "Select PNG images to combine",
            InitialDirectory = DataStorage.GetDataFolder()
        };

        if (openDialog.ShowDialog() != true || openDialog.FileNames.Length == 0)
            return;

        CombineImagesPanel.Children.Clear();
        foreach (var file in openDialog.FileNames.OrderBy(f => f))
        {
            var ctrl = CreateCombineImageControl(file);
            CombineImagesPanel.Children.Add(ctrl);
        }
        StatusText.Text = $"Selected {CombineImagesPanel.Children.Count} images for combine.";
    }

    private CombineImageItemControl CreateCombineImageControl(string filePath)
    {
        var ctrl = new CombineImageItemControl(filePath);
        ctrl.DeleteRequested += (s, _) =>
        {
            if (s is CombineImageItemControl toRemove)
                CombineImagesPanel.Children.Remove(toRemove);
        };
        ctrl.DragReorderRequested += OnCombineDragReorderRequested;
        return ctrl;
    }

    private void OnCombineDragReorderRequested(object? sender, CombineImageItemControl control)
    {
        if (_draggedCombineControl != null) return;
        _draggedCombineControl = control;
        Mouse.Capture(control, CaptureMode.SubTree);
        control.PreviewMouseMove += OnCombineDragMouseMove;
        control.PreviewMouseLeftButtonUp += OnCombineDragMouseUp;
    }

    private void OnCombineDragMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedCombineControl == null) return;
        var panel = CombineImagesPanel;
        var pt = e.GetPosition(panel);
        var hit = VisualTreeHelper.HitTest(panel, pt);
        CombineImageItemControl? targetCtrl = null;
        for (var v = hit?.VisualHit as DependencyObject; v != null; v = VisualTreeHelper.GetParent(v))
        {
            if (v is CombineImageItemControl c)
            {
                targetCtrl = c;
                break;
            }
        }
        if (targetCtrl != null && targetCtrl != _draggedCombineControl)
        {
            var fromIndex = panel.Children.IndexOf(_draggedCombineControl);
            var targetIndex = panel.Children.IndexOf(targetCtrl);
            if (fromIndex >= 0 && targetIndex >= 0 && fromIndex != targetIndex)
            {
                panel.Children.RemoveAt(fromIndex);
                panel.Children.Insert(targetIndex, _draggedCombineControl);
            }
        }
    }

    private void OnCombineDragMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggedCombineControl == null) return;
        _draggedCombineControl.PreviewMouseMove -= OnCombineDragMouseMove;
        _draggedCombineControl.PreviewMouseLeftButtonUp -= OnCombineDragMouseUp;
        Mouse.Capture(null);
        _draggedCombineControl = null;
    }

    private void CombineImages_Click(object sender, RoutedEventArgs e)
    {
        var filePaths = CombineImagesPanel.Children
            .OfType<CombineImageItemControl>()
            .Select(c => c.FilePath)
            .ToArray();

        if (filePaths.Length == 0)
        {
            StatusText.Text = "Select images first (click 'Select Images').";
            return;
        }

        if (!int.TryParse(CombineColumnsBox.Text, out var columns) || columns < 1)
        {
            StatusText.Text = "Enter a valid column count (1 or more).";
            return;
        }

        var images = new List<BitmapSource>();
        foreach (var file in filePaths)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(file);
                bitmap.EndInit();
                bitmap.Freeze();
                images.Add(bitmap);
            }
            catch
            {
                // Skip invalid images
            }
        }

        if (images.Count == 0)
        {
            StatusText.Text = "Could not load any images.";
            return;
        }

        try
        {
            var combined = ImageGridCombiner.Combine(images, columns);

            var folder = Path.GetDirectoryName(filePaths[0]) ?? DataStorage.GetDataFolder();
            var saveDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|All Files|*.*",
                DefaultExt = "png",
                FileName = "combined.png",
                InitialDirectory = folder
            };

            if (saveDialog.ShowDialog() == true)
            {
                AimPointImageGenerator.SaveToFile(combined, saveDialog.FileName);
                StatusText.Text = $"Combined {images.Count} images ({columns} cols): {Path.GetFileName(saveDialog.FileName)}";
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
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
        var currentFileName = $"{data.GetFileName()}.json";
        var savePath = DataStorage.GetDefaultFilePath(data);

        if (string.IsNullOrEmpty(_lastLoadedPath))
        {
            SaveDataAs_Click(sender, e);
            return;
        }

        var lastFileName = Path.GetFileName(_lastLoadedPath);
        var namesChanged = !string.Equals(currentFileName, lastFileName, StringComparison.OrdinalIgnoreCase);

        if (namesChanged)
        {
            DataStorage.Save(data, savePath);
            _lastLoadedPath = savePath;
            StatusText.Text = $"Saved: {Path.GetFileName(savePath)}";
            MessageBox.Show($"Saved: {Path.GetFileName(savePath)}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var result = MessageBox.Show(
                $"Overwrite the current file?\n\n{savePath}",
                "Save Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            DataStorage.Save(data, savePath);
            StatusText.Text = $"Saved: {Path.GetFileName(savePath)}";
            MessageBox.Show($"Saved: {Path.GetFileName(savePath)}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
        MessageBox.Show($"Saved: {Path.GetFileName(dialog.FileName)}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
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

    private void OnEffectiveRangeChanged()
    {
        if (TryParseNumericInput(EffectiveRangeBox.Text, out _, out var normalized)
            && EffectiveRangeBox.Text != normalized)
        {
            EffectiveRangeBox.Text = normalized;
            EffectiveRangeBox.SelectionStart = EffectiveRangeBox.Text.Length;
        }

        RefreshImage();
    }

    private static bool TryParseNumericInput(string raw, out double value, out string normalized)
    {
        value = 0;
        normalized = raw;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var input = raw.Trim();
        if (TryEvaluateExpression(input, out value))
        {
            normalized = value.ToString("0.###############", CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }

    private static bool TryEvaluateExpression(string input, out double value)
    {
        value = 0;
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
}
