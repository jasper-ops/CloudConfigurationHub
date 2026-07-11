namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 配置定义摘要。
/// </summary>
/// <param name="Id">配置定义 ID。</param>
/// <param name="Group">配置分组。</param>
/// <param name="Key">配置 Key。</param>
/// <param name="IsSensitive">是否为敏感配置。</param>
public sealed record ConfigurationDefinitionSummary(Guid Id, string Group, string Key, bool IsSensitive);
