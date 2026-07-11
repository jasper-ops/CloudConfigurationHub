using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 从历史发布版本回滚环境配置的命令。
/// </summary>
/// <param name="ProjectId">项目 ID。</param>
/// <param name="EnvironmentId">回滚目标环境 ID。</param>
/// <param name="SourceReleaseId">作为回滚源的历史发布版本 ID。</param>
/// <param name="Note">回滚发布备注。</param>
/// <param name="PublishedBy">执行回滚的管理员标识。</param>
public sealed record RollbackEnvironmentCommand(
    Guid ProjectId,
    Guid EnvironmentId,
    Guid SourceReleaseId,
    string Note,
    string PublishedBy) : ICommand<ConfigurationReleaseSummary>;
