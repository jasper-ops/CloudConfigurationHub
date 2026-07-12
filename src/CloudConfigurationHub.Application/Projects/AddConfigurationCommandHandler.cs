using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理添加配置定义命令。
/// </summary>
/// <param name="repository">项目仓储端口。</param>
/// <param name="logger">结构化日志记录器，用于记录配置定义管理审计事件。</param>
public sealed class AddConfigurationCommandHandler(
    IProjectRepository repository,
    ILogger<AddConfigurationCommandHandler> logger)
    : ICommandHandler<AddConfigurationCommand, ConfigurationDefinitionSummary> {
    /// <summary>
    /// 添加配置定义并保存项目聚合。
    /// </summary>
    /// <param name="command">添加配置定义命令。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取和保存操作。</param>
    /// <returns>新建配置定义摘要。</returns>
    /// <exception cref="DomainException">当项目不存在或分组 Key 重复时抛出。</exception>
    public async ValueTask<ConfigurationDefinitionSummary> Handle(
        AddConfigurationCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");
        var configuration = project.AddConfiguration(
            command.Group,
            command.Key,
            command.IsSensitive,
            command.Description);
        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已添加配置定义。ProjectId={ProjectId}, ConfigurationId={ConfigurationId}, Group={Group}, Key={Key}, IsSensitive={IsSensitive}",
            project.Id,
            configuration.Id,
            configuration.Group,
            configuration.Key,
            configuration.IsSensitive);
        return new ConfigurationDefinitionSummary(
            configuration.Id,
            configuration.Group,
            configuration.Key,
            configuration.IsSensitive,
            configuration.Description);
    }
}
