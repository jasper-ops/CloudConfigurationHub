using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 更新配置项目基础信息的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="Name">项目显示名称。</param>
/// <param name="Key">项目唯一 Key。</param>
/// <param name="Description">项目说明。</param>
public sealed record UpdateProjectCommand(Guid ProjectId, string Name, string Key, string Description)
    : ICommand<ProjectSummary>;
