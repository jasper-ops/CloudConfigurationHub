using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理删除配置项目命令。
/// </summary>
public sealed class DeleteProjectCommandHandler(
    IProjectRepository repository,
    ILogger<DeleteProjectCommandHandler> logger) : ICommandHandler<DeleteProjectCommand> {
    /// <summary>
    /// 从仓储删除项目聚合。
    /// </summary>
    public async ValueTask<Unit> Handle(DeleteProjectCommand command, CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");

        await repository.DeleteAsync(project, cancellationToken);
        logger.LogInformation("已删除配置项目。ProjectId={ProjectId}, ProjectKey={ProjectKey}", project.Id, project.Key);
        return Unit.Value;
    }
}
