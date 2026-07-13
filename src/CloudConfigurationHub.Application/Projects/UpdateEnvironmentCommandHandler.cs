using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理更新项目环境命令。
/// </summary>
public sealed class UpdateEnvironmentCommandHandler(
    IProjectRepository repository,
    ILogger<UpdateEnvironmentCommandHandler> logger)
    : ICommandHandler<UpdateEnvironmentCommand, EnvironmentSummary> {
    /// <summary>
    /// 更新项目环境基础信息并保存项目聚合。
    /// </summary>
    public async ValueTask<EnvironmentSummary> Handle(
        UpdateEnvironmentCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");

        var environment = project.UpdateEnvironment(command.EnvironmentId, command.Name, command.Key);
        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已更新项目环境。ProjectId={ProjectId}, EnvironmentId={EnvironmentId}, EnvironmentKey={EnvironmentKey}, EnvironmentName={EnvironmentName}",
            project.Id,
            environment.Id,
            environment.Key,
            environment.Name);

        return new EnvironmentSummary(environment.Id, environment.Name, environment.Key);
    }
}
