using System.Windows;
using System.Windows.Media;
using AimPointGenerator.Models;
using AimPointGenerator.Services;

namespace AimPointGenerator.Services;

/// <summary>
/// 테마 적용 및 리소스 관리
/// </summary>
public static class ThemeManager
{
    public const string KeyWindowBackground = "ThemeWindowBackground";
    public const string KeyPanelBackground = "ThemePanelBackground";
    public const string KeyPanelInner = "ThemePanelInner";
    public const string KeyTitleForeground = "ThemeTitleForeground";
    public const string KeyLabelForeground = "ThemeLabelForeground";
    public const string KeySectionHeaderForeground = "ThemeSectionHeaderForeground";
    public const string KeyTextBoxBackground = "ThemeTextBoxBackground";
    public const string KeyTextBoxForeground = "ThemeTextBoxForeground";
    public const string KeyTextBoxBorder = "ThemeTextBoxBorder";
    public const string KeyButtonPrimary = "ThemeButtonPrimary";
    public const string KeyButtonDelete = "ThemeButtonDelete";
    public const string KeyBorderColor = "ThemeBorderColor";
    public const string KeyItemBackground = "ThemeItemBackground";
    public const string KeyItemDragHandle = "ThemeItemDragHandle";
    public const string KeyStatusForeground = "ThemeStatusForeground";
    public const string KeyScrollBarBackground = "ThemeScrollBarBackground";
    public const string KeyScrollBarThumb = "ThemeScrollBarThumb";
    public const string KeyScrollBarThumbHover = "ThemeScrollBarThumbHover";

    private static ThemeData _current = new();

    public static ThemeData Current => _current;

    public static event Action? ThemeChanged;

    public static void Load()
    {
        var loaded = ThemeStorage.Load();
        if (loaded != null)
            _current = loaded;
        Apply();
    }

    public static void Apply(ThemeData? theme = null)
    {
        if (theme != null)
            _current = theme;

        var app = Application.Current;
        if (app == null) return;

        var res = app.Resources;
        res[KeyWindowBackground] = Brush(_current.WindowBackground);
        res[KeyPanelBackground] = Brush(_current.PanelBackground);
        res[KeyPanelInner] = Brush(_current.PanelInner);
        res[KeyTitleForeground] = Brush(_current.TitleForeground);
        res[KeyLabelForeground] = Brush(_current.LabelForeground);
        res[KeySectionHeaderForeground] = Brush(_current.SectionHeaderForeground);
        res[KeyTextBoxBackground] = Brush(_current.TextBoxBackground);
        res[KeyTextBoxForeground] = Brush(_current.TextBoxForeground);
        res[KeyTextBoxBorder] = Brush(_current.TextBoxBorder);
        res[KeyButtonPrimary] = Brush(_current.ButtonPrimary);
        res[KeyButtonDelete] = Brush(_current.ButtonDelete);
        res[KeyBorderColor] = Brush(_current.BorderColor);
        res[KeyItemBackground] = Brush(_current.ItemBackground);
        res[KeyItemDragHandle] = Brush(_current.ItemDragHandle);
        res[KeyStatusForeground] = Brush(_current.StatusForeground);
        res[KeyScrollBarBackground] = Brush(_current.ScrollBarBackground);
        res[KeyScrollBarThumb] = Brush(_current.ScrollBarThumb);
        res[KeyScrollBarThumbHover] = Brush(_current.ScrollBarThumbHover);

        ThemeChanged?.Invoke();
    }

    public static void Save()
    {
        ThemeStorage.Save(_current);
    }

    private static SolidColorBrush Brush(string? hex)
    {
        return new SolidColorBrush(ColorHelper.ParseHex(hex));
    }
}
