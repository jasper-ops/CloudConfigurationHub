using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理发布项目环境配置命令。
/// </summary>
/// <param name="repository">项目仓储端口。</param>
/// <param name="broadcaster">配置变更广播器。</param>
/// <param name="clock">应用层时钟。</param>
/// <param name="logger">结构化日志记录器，用于记录发布审计事件。</param>
public sealed class PublishEnvironmentCommandHandler(
    IProjectRepository repository,
    IConfigurationChangeBroadcaster broadcaster,
    IClock clock,
    ILogger<PublishEnvironmentCommandHandler> logger)
    : ICommandHandler<PublishEnvironmentCommand, ConfigurationReleaseSummary> {
    /// <summary>
    /// 发布环境配置、保存聚合并广播版本变更事件。
    /// </summary>
    /// <param name="command">发布环境配置命令。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取、保存和广播。</param>
    /// <returns>新发布版本摘要。</returns>
    /// <exception cref="DomainException">当项目或环境不存在时抛出。</exception>
    public async ValueTask<ConfigurationReleaseSummary> Handle(
        PublishEnvironmentCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");
        var environment = project.Environments.SingleOrDefault(item => item.Id == command.EnvironmentId)
            ?? throw new DomainException("环境不存在。");
        var release = project.PublishEnvironment(
            command.EnvironmentId,
            command.Note,
            command.PublishedBy,
            clock.UtcNow);
        await repository.SaveChangesAsync(project, cancellationToken);
        await broadcaster.PublishAsync(
            new ConfigurationChangedEvent(project.Key, environment.Key, release.Version),
            cancellationToken);
        logger.LogInformation(
            "已发布环境配置。ProjectId={ProjectId}, ProjectKey={ProjectKey}, EnvironmentId={EnvironmentId}, EnvironmentKey={EnvironmentKey}, Version={Version}, PublishedBy={PublishedBy}, ValueCount={ValueCount}",
            project.Id,
            project.Key,
            environment.Id,
            environment.Key,
            release.Version,
            release.PublishedBy,
            release.Values.Count);
        return new ConfigurationReleaseSummary(
            release.Id,
            release.EnvironmentId,
            release.Version,
            release.Note,
            release.PublishedBy,
            release.PublishedAt,
            release.Values
                .Select(item => new ConfigurationReleaseValueSummary(
                    item.ConfigurationId,
                    item.ConfigurationKey,
                    item.IsSensitive ? "******" : item.Value,
                    item.IsSensitive))
                .ToArray());
    }
}
