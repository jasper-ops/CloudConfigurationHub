using CloudConfigurationHub.Application;

namespace CloudConfigurationHub.Infrastructure;

/// <summary>
/// 使用系统时间的应用层时钟实现。
/// </summary>
public sealed class SystemClock : IClock {
    /// <summary>
    /// 当前 UTC 时间。
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
