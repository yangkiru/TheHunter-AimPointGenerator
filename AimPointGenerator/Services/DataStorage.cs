using System.IO;
using System.Text.Json;
using AimPointGenerator.Models;

namespace AimPointGenerator.Services;

/// <summary>
/// AimPoint 데이터 저장/불러오기
/// 데이터는 실행파일 위치의 AimPointData 폴더에 저장
/// </summary>
public static class DataStorage
{
    private const string DataFolderName = "AimPointData";
    private const string LastFileKey = "lastfile.txt";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 실행파일이 있는 폴더 경로
    /// </summary>
    public static string GetExeDirectory()
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        return Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }

    /// <summary>
    /// 데이터 저장 폴더 (실행파일 위치/AimPointData)
    /// </summary>
    public static string GetDataFolder()
    {
        var folder = Path.Combine(GetExeDirectory(), DataFolderName);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return folder;
    }

    /// <summary>
    /// 마지막으로 불러온/저장한 파일 경로
    /// </summary>
    public static string? GetLastFilePath()
    {
        var pathFile = Path.Combine(GetDataFolder(), LastFileKey);
        if (!File.Exists(pathFile)) return null;
        try
        {
            var path = File.ReadAllText(pathFile).Trim();
            return string.IsNullOrEmpty(path) ? null : path;
        }
        catch { return null; }
    }

    public static void SetLastFilePath(string filePath)
    {
        var pathFile = Path.Combine(GetDataFolder(), LastFileKey);
        File.WriteAllText(pathFile, filePath);
    }

    public static void Save(AimPointData data, string filePath)
    {
        var json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(filePath, json);
        SetLastFilePath(filePath);
    }

    public static AimPointData? Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<AimPointData>(json, Options);
        if (data != null)
            SetLastFilePath(filePath);
        return data;
    }

    public static string GetDefaultFilePath(AimPointData data)
    {
        return Path.Combine(GetDataFolder(), $"{data.GetFileName()}.json");
    }

    public static string GetDefaultFileName(AimPointData data)
    {
        return $"{data.GetFileName()}.json";
    }
}
