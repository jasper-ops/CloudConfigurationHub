namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端某环境下的配置草稿显示值。
/// </summary>
/// <param name="EnvironmentId">环境 ID。</param>
/// <param name="EnvironmentKey">环境 Key。</param>
/// <param name="DisplayValue">管理端显示值；敏感配置固定为掩码。</param>
/// <param name="HasValue">是否已经设置草稿值。</param>
/// <param name="PublicationState">草稿值相对最新发布版本的状态。</param>
/// <param name="LatestPublishedDisplayValue">最新发布版本中的显示值；敏感配置固定为掩码。</param>
/// <param name="LatestPublishedVersion">最新发布版本号；没有发布版本时为 <see langword="null"/>。</param>
/// <param name="LatestPublishedAt">最新发布时间；没有发布版本时为 <see langword="null"/>。</param>
public sealed record EnvironmentDraftValue(
    Guid EnvironmentId,
    string EnvironmentKey,
    string DisplayValue,
    bool HasValue,
    ConfigurationValuePublicationState PublicationState = ConfigurationValuePublicationState.NotSet,
    string LatestPublishedDisplayValue = "",
    int? LatestPublishedVersion = null,
    DateTimeOffset? LatestPublishedAt = null);
