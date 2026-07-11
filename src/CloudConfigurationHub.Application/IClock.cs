namespace CloudConfigurationHub.Application;

/// <summary>
/// 应用层时钟端口。
/// </summary>
public interface IClock {
    /// <summary>
    /// 当前 UTC 时间。
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
