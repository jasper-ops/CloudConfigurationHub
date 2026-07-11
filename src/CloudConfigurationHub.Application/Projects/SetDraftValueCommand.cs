using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 保存某环境某配置项草稿值的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="EnvironmentId">环境 ID。</param>
/// <param name="ConfigurationId">配置定义 ID。</param>
/// <param name="Value">草稿字符串值。</param>
public sealed record SetDraftValueCommand(Guid ProjectId, Guid EnvironmentId, Guid ConfigurationId, string Value)
    : ICommand;
