using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 发布项目环境配置的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="EnvironmentId">发布目标环境 ID。</param>
/// <param name="Note">发布备注。</param>
/// <param name="PublishedBy">发布人标识。</param>
public sealed record PublishEnvironmentCommand(Guid ProjectId, Guid EnvironmentId, string Note, string PublishedBy)
    : ICommand<ConfigurationReleaseSummary>;
