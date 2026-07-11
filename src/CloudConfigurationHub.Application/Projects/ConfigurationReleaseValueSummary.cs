namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端可见的发布版本配置值摘要。
/// </summary>
/// <param name="ConfigurationId">配置定义 ID。</param>
/// <param name="ConfigurationKey">发布快照中的完整配置 Key。</param>
/// <param name="DisplayValue">管理端展示值，敏感配置必须脱敏。</param>
/// <param name="IsSensitive">指示该值是否来自敏感配置定义。</param>
public sealed record ConfigurationReleaseValueSummary(
    Guid ConfigurationId,
    string ConfigurationKey,
    string DisplayValue,
    bool IsSensitive);
