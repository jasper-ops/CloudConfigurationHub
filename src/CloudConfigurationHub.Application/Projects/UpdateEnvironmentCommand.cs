using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 更新项目环境基础信息的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="EnvironmentId">环境 ID。</param>
/// <param name="Name">环境显示名称。</param>
/// <param name="Key">环境唯一 Key。</param>
public sealed record UpdateEnvironmentCommand(Guid ProjectId, Guid EnvironmentId, string Name, string Key)
    : ICommand<EnvironmentSummary>;
