using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 删除配置定义的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="ConfigurationId">配置定义 ID。</param>
public sealed record DeleteConfigurationCommand(Guid ProjectId, Guid ConfigurationId) : ICommand;
