namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端项目列表查询结果。
/// </summary>
/// <param name="Projects">项目卡片集合。</param>
public sealed record ProjectListResult(IReadOnlyList<ProjectCard> Projects);
