namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端配置定义详情。
/// </summary>
/// <param name="Id">配置定义 ID。</param>
/// <param name="Group">配置分组。</param>
/// <param name="Key">配置 Key。</param>
/// <param name="IsSensitive">是否为敏感配置。</param>
/// <param name="Values">该配置在各环境下的草稿显示值。</param>
public sealed record ConfigurationDetail(
    Guid Id,
    string Group,
    string Key,
    bool IsSensitive,
    IReadOnlyList<EnvironmentDraftValue> Values);
