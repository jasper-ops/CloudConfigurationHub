using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理环境配置回滚命令。
/// </summary>
/// <param name="repository">项目仓储端口。</param>
/// <param name="broadcaster">配置变更广播器。</param>
/// <param name="clock">应用层时钟。</param>
/// <param name="logger">结构化日志记录器，用于记录回滚审计事件。</param>
public sealed class RollbackEnvironmentCommandHandler(
    IProjectRepository repository,
    IConfigurationChangeBroadcaster broadcaster,
    IClock clock,
    ILogger<RollbackEnvironmentCommandHandler> logger)
    : ICommandHandler<RollbackEnvironmentCommand, ConfigurationReleaseSummary> {
    /// <summary>
    /// 基于历史发布版本创建新的发布版本，并广播配置版本变更。
    /// </summary>
    /// <param name="command">环境配置回滚命令。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取、保存和广播。</param>
    /// <returns>回滚生成的新发布版本摘要。</returns>
    /// <exception cref="DomainException">当项目、环境或回滚源版本不存在时抛出。</exception>
    public async ValueTask<ConfigurationReleaseSummary> Handle(
        RollbackEnvironmentCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");
        var environment = project.Environments.SingleOrDefault(item => item.Id == command.EnvironmentId)
            ?? throw new DomainException("环境不存在。");
        var release = project.RollbackEnvironment(
            command.EnvironmentId,
            command.SourceReleaseId,
            command.Note,
            command.PublishedBy,
            clock.UtcNow);
        await repository.SaveChangesAsync(project, cancellationToken);
        await broadcaster.PublishAsync(
            new ConfigurationChangedEvent(project.Key, environment.Key, release.Version),
            cancellationToken);
        logger.LogInformation(
            "已回滚环境配置。ProjectId={ProjectId}, ProjectKey={ProjectKey}, EnvironmentId={EnvironmentId}, EnvironmentKey={EnvironmentKey}, SourceReleaseId={SourceReleaseId}, Version={Version}, PublishedBy={PublishedBy}, ValueCount={ValueCount}",
            project.Id,
            project.Key,
            environment.Id,
            environment.Key,
            command.SourceReleaseId,
            release.Version,
            release.PublishedBy,
            release.Values.Count);
        return new ConfigurationReleaseSummary(
            release.Id,
            release.EnvironmentId,
            release.Version,
            release.Note,
            release.PublishedBy,
            release.PublishedAt);
    }
}
