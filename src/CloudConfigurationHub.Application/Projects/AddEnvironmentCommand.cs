using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 为项目添加环境的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="Name">环境显示名称。</param>
/// <param name="Key">环境唯一 Key。</param>
public sealed record AddEnvironmentCommand(Guid ProjectId, string Name, string Key) : ICommand<EnvironmentSummary>;
