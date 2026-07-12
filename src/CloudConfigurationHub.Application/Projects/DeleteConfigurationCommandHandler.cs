using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理删除配置定义命令。
/// </summary>
public sealed class DeleteConfigurationCommandHandler(
    IProjectRepository repository,
    ILogger<DeleteConfigurationCommandHandler> logger) : ICommandHandler<DeleteConfigurationCommand> {
    /// <summary>
    /// 删除配置定义及其草稿值并保存项目聚合。
    /// </summary>
    public async ValueTask<Unit> Handle(DeleteConfigurationCommand command, CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");

        project.RemoveConfiguration(command.ConfigurationId);
        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已删除配置定义。ProjectId={ProjectId}, ConfigurationId={ConfigurationId}",
            project.Id,
            command.ConfigurationId);
        return Unit.Value;
    }
}
