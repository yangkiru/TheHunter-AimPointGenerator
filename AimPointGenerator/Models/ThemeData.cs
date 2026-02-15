namespace AimPointGenerator.Models;

/// <summary>
/// 앱 및 생성 이미지 테마 색상
/// </summary>
public class ThemeData
{
    // === 앱 UI ===
    public string WindowBackground { get; set; } = "#1a2a1a";
    public string PanelBackground { get; set; } = "#0d1f0d";
    public string PanelInner { get; set; } = "#0a150a";
    public string TitleForeground { get; set; } = "#90EE90";
    public string LabelForeground { get; set; } = "#90EE90";
    public string SectionHeaderForeground { get; set; } = "#7CCD7C";
    public string TextBoxBackground { get; set; } = "#0d1f0d";
    public string TextBoxForeground { get; set; } = "#90EE90";
    public string TextBoxBorder { get; set; } = "#2d5a2d";
    public string ButtonPrimary { get; set; } = "#2d5a2d";
    public string ButtonDelete { get; set; } = "#5a2020";
    public string BorderColor { get; set; } = "#2d5a2d";
    public string ItemBackground { get; set; } = "#152515";
    public string ItemDragHandle { get; set; } = "#1a3a1a";
    public string StatusForeground { get; set; } = "#FF6B6B";
    public string ScrollBarBackground { get; set; } = "#0d1f0d";
    public string ScrollBarThumb { get; set; } = "#2d4a2d";
    public string ScrollBarThumbHover { get; set; } = "#3d5a3d";

    // === 생성 이미지 ===
    public string ImageReticle { get; set; } = "#00FF64";      // RGB(0,255,100)
    public string ImageText { get; set; } = "#C8FFC8";         // RGB(200,255,200)
    public string ImageBackground { get; set; } = "#DC001900"; // ARGB(220,0,25,0)
}
