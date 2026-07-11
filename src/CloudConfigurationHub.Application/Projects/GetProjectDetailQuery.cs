using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 查询管理端项目配置详情。
/// </summary>
/// <param name="ProjectId">项目 ID。</param>
public sealed record GetProjectDetailQuery(Guid ProjectId) : IQuery<ProjectDetail?>;
