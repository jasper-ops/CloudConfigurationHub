using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理更新配置定义命令。
/// </summary>
public sealed class UpdateConfigurationCommandHandler(
    IProjectRepository repository,
    ILogger<UpdateConfigurationCommandHandler> logger)
    : ICommandHandler<UpdateConfigurationCommand, ConfigurationDefinitionSummary> {
    /// <summary>
    /// 更新配置定义，并同步传入的环境草稿值。
    /// </summary>
    public async ValueTask<ConfigurationDefinitionSummary> Handle(
        UpdateConfigurationCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");

        var configuration = project.UpdateConfiguration(
            command.ConfigurationId,
            command.Group,
            command.Key,
            command.IsSensitive,
            command.Description);
        foreach (var (environmentId, value) in command.EnvironmentValues) {
            project.SetDraftValue(environmentId, configuration.Id, value);
        }

        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已更新配置定义。ProjectId={ProjectId}, ConfigurationId={ConfigurationId}, Group={Group}, Key={Key}",
            project.Id,
            configuration.Id,
            configuration.Group,
            configuration.Key);

        return new ConfigurationDefinitionSummary(
            configuration.Id,
            configuration.Group,
            configuration.Key,
            configuration.IsSensitive,
            configuration.Description);
    }
}
