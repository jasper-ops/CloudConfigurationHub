using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理保存配置草稿值命令。
/// </summary>
/// <param name="repository">项目仓储端口。</param>
/// <param name="logger">结构化日志记录器，用于记录草稿编辑审计事件。</param>
public sealed class SetDraftValueCommandHandler(
    IProjectRepository repository,
    ILogger<SetDraftValueCommandHandler> logger) : ICommandHandler<SetDraftValueCommand> {
    /// <summary>
    /// 保存某环境某配置项的草稿值。
    /// </summary>
    /// <param name="command">保存草稿值命令。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取和保存操作。</param>
    /// <returns>表示异步处理的值任务。</returns>
    /// <exception cref="DomainException">当项目、环境或配置项不存在时抛出。</exception>
    public async ValueTask<Unit> Handle(SetDraftValueCommand command, CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");
        project.SetDraftValue(command.EnvironmentId, command.ConfigurationId, command.Value);
        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已保存配置草稿值。ProjectId={ProjectId}, EnvironmentId={EnvironmentId}, ConfigurationId={ConfigurationId}, ValueLength={ValueLength}",
            project.Id,
            command.EnvironmentId,
            command.ConfigurationId,
            command.Value.Length);
        return Unit.Value;
    }
}
