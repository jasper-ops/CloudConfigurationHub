namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端某环境下的配置草稿显示值。
/// </summary>
/// <param name="EnvironmentId">环境 ID。</param>
/// <param name="EnvironmentKey">环境 Key。</param>
/// <param name="DisplayValue">管理端显示值；敏感配置固定为掩码。</param>
/// <param name="HasValue">是否已经设置草稿值。</param>
public sealed record EnvironmentDraftValue(
    Guid EnvironmentId,
    string EnvironmentKey,
    string DisplayValue,
    bool HasValue);
