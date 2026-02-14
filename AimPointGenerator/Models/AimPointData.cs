namespace AimPointGenerator.Models;

/// <summary>
/// AimPoint 데이터 (조준경 + 조준점)
/// </summary>
public class AimPointData
{
    /// <summary>
    /// 탄환 이름
    /// </summary>
    public string AmmunitionName { get; set; } = string.Empty;

    /// <summary>
    /// 탄환 유효 사거리 (미터)
    /// </summary>
    public double EffectiveRange { get; set; }

    /// <summary>
    /// 조준경 이름
    /// </summary>
    public string ScopeName { get; set; } = string.Empty;

    /// <summary>
    /// 조준점 데이터 리스트
    /// </summary>
    public List<AimPointItem> AimPointItems { get; set; } = new();

    /// <summary>
    /// 파일명 생성 (탄환이름_조준경이름)
    /// </summary>
    public string GetFileName()
    {
        var ammo = string.IsNullOrWhiteSpace(AmmunitionName) ? "default" : AmmunitionName.Replace("/", "-");
        var scope = string.IsNullOrWhiteSpace(ScopeName) ? "scope" : ScopeName.Replace("/", "-");
        return $"{ammo}_{scope}";
    }
}
