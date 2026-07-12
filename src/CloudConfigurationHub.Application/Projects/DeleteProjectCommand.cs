using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 删除配置项目的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
public sealed record DeleteProjectCommand(Guid ProjectId) : ICommand;
