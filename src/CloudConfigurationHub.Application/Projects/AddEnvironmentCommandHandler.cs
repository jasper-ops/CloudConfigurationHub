using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理添加项目环境命令。
/// </summary>
/// <param name="repository">项目仓储端口。</param>
/// <param name="logger">结构化日志记录器，用于记录环境管理审计事件。</param>
public sealed class AddEnvironmentCommandHandler(
    IProjectRepository repository,
    ILogger<AddEnvironmentCommandHandler> logger) : ICommandHandler<AddEnvironmentCommand, EnvironmentSummary> {
    /// <summary>
    /// 添加项目环境并保存项目聚合。
    /// </summary>
    /// <param name="command">添加环境命令。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取和保存操作。</param>
    /// <returns>新建环境摘要。</returns>
    /// <exception cref="DomainException">当项目不存在或环境 Key 重复时抛出。</exception>
    public async ValueTask<EnvironmentSummary> Handle(
        AddEnvironmentCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");
        var environment = project.AddEnvironment(command.Name, command.Key);
        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已添加项目环境。ProjectId={ProjectId}, EnvironmentId={EnvironmentId}, EnvironmentKey={EnvironmentKey}, EnvironmentName={EnvironmentName}",
            project.Id,
            environment.Id,
            environment.Key,
            environment.Name);
        return new EnvironmentSummary(environment.Id, environment.Name, environment.Key);
    }
}
