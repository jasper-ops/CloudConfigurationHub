namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 项目环境摘要。
/// </summary>
/// <param name="Id">环境 ID。</param>
/// <param name="Name">环境显示名称。</param>
/// <param name="Key">环境唯一 Key。</param>
public sealed record EnvironmentSummary(Guid Id, string Name, string Key);
