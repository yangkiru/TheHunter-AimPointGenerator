namespace AimPointGenerator.Models;

/// <summary>
/// 조준점 데이터 항목
/// </summary>
public class AimPointItem
{
    /// <summary>
    /// 타겟 거리 (미터)
    /// </summary>
    public double TargetDistance { get; set; }

    /// <summary>
    /// 영점 거리 (미터)
    /// </summary>
    public double ZeroingDistance { get; set; }

    /// <summary>
    /// 조준 위치 (중앙부터 몇 번째 점인지, 실수 값)
    /// </summary>
    public double AimPosition { get; set; }

    /// <summary>
    /// true = Min Zoom, false = Max Zoom
    /// </summary>
    public bool IsMinZoom { get; set; }
}
