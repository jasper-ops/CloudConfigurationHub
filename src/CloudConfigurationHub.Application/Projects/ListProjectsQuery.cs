using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 查询管理端项目列表。
/// </summary>
public sealed record ListProjectsQuery : IQuery<ProjectListResult>;
