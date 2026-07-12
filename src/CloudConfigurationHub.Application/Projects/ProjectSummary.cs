namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 项目摘要视图。
/// </summary>
/// <param name="Id">项目聚合 ID。</param>
/// <param name="Name">项目显示名称。</param>
/// <param name="Key">项目唯一 Key。</param>
/// <param name="Description">项目说明。</param>
public sealed record ProjectSummary(Guid Id, string Name, string Key, string Description = "");
