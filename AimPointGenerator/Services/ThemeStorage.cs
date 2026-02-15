using System.IO;
using System.Text.Json;
using AimPointGenerator.Models;

namespace AimPointGenerator.Services;

/// <summary>
/// 테마 저장/불러오기
/// </summary>
public static class ThemeStorage
{
    private const string ThemeFileName = "theme.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string GetThemePath()
    {
        return Path.Combine(DataStorage.GetDataFolder(), ThemeFileName);
    }

    public static ThemeData? Load()
    {
        var path = GetThemePath();
        if (!File.Exists(path)) return null;
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ThemeData>(json, Options);
        }
        catch
        {
            return null;
        }
    }

    public static void Save(ThemeData theme)
    {
        var path = GetThemePath();
        var json = JsonSerializer.Serialize(theme, Options);
        File.WriteAllText(path, json);
    }
}
