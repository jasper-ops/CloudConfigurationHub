using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理更新配置项目基础信息命令。
/// </summary>
public sealed class UpdateProjectCommandHandler(
    IProjectRepository repository,
    ILogger<UpdateProjectCommandHandler> logger)
    : ICommandHandler<UpdateProjectCommand, ProjectSummary> {
    /// <summary>
    /// 更新项目基础信息并保存项目聚合。
    /// </summary>
    public async ValueTask<ProjectSummary> Handle(UpdateProjectCommand command, CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");

        project.UpdateDetails(command.Name, command.Key, command.Description);
        await repository.SaveChangesAsync(project, cancellationToken);

        logger.LogInformation(
            "已更新配置项目。ProjectId={ProjectId}, ProjectKey={ProjectKey}, ProjectName={ProjectName}",
            project.Id,
            project.Key,
            project.Name);

        return new ProjectSummary(project.Id, project.Name, project.Key, project.Description);
    }
}
