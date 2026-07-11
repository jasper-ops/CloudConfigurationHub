using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理创建配置项目的命令。
/// </summary>
/// <param name="repository">项目仓储端口，用于持久化新创建的项目聚合。</param>
/// <param name="logger">结构化日志记录器，用于记录项目创建审计事件。</param>
public sealed class CreateProjectCommandHandler(
    IProjectRepository repository,
    ILogger<CreateProjectCommandHandler> logger)
    : ICommandHandler<CreateProjectCommand, ProjectSummary> {
    /// <summary>
    /// 创建项目聚合、保存到仓储，并输出包含项目上下文的审计日志。
    /// </summary>
    /// <param name="command">创建项目命令，包含名称和项目 Key。</param>
    /// <param name="cancellationToken">取消令牌，用于终止仓储写入。</param>
    /// <returns>创建后的项目摘要。</returns>
    public async ValueTask<ProjectSummary> Handle(
        CreateProjectCommand command,
        CancellationToken cancellationToken) {
        var project = Project.Create(command.Name, command.Key);

        await repository.AddAsync(project, cancellationToken);

        logger.LogInformation(
            "已创建配置项目。ProjectId={ProjectId}, ProjectKey={ProjectKey}, ProjectName={ProjectName}",
            project.Id,
            project.Key,
            project.Name);

        return new ProjectSummary(project.Id, project.Name, project.Key);
    }
}
