using CloudConfigurationHub.Application.Security;
using CloudConfigurationHub.Domain.Projects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理项目级只读 Access Key 轮换命令。
/// </summary>
/// <param name="repository">项目仓储端口。</param>
/// <param name="accessKeyGenerator">Access Key 明文生成器。</param>
/// <param name="accessKeyHasher">Access Key 哈希器。</param>
/// <param name="logger">结构化日志记录器，用于记录密钥轮换审计事件。</param>
public sealed class RotateProjectAccessKeyCommandHandler(
    IProjectRepository repository,
    IAccessKeyGenerator accessKeyGenerator,
    IAccessKeyHasher accessKeyHasher,
    ILogger<RotateProjectAccessKeyCommandHandler> logger)
    : ICommandHandler<RotateProjectAccessKeyCommand, ProjectAccessKeyRotationResult> {
    /// <summary>
    /// 生成新的 Access Key、持久化哈希值，并返回只展示一次的明文。
    /// </summary>
    /// <param name="command">轮换项目 Access Key 命令。</param>
    /// <param name="cancellationToken">取消令牌，用于终止仓储读取与保存。</param>
    /// <returns>包含项目标识与新 Access Key 明文的轮换结果。</returns>
    /// <exception cref="DomainException">当项目不存在时抛出。</exception>
    public async ValueTask<ProjectAccessKeyRotationResult> Handle(
        RotateProjectAccessKeyCommand command,
        CancellationToken cancellationToken) {
        var project = await repository.GetByIdAsync(command.ProjectId, cancellationToken)
            ?? throw new DomainException("项目不存在。");
        var accessKey = accessKeyGenerator.Generate();
        var accessKeyHash = accessKeyHasher.Hash(accessKey);
        project.ReplaceAccessKeyHash(accessKeyHash);
        await repository.SaveChangesAsync(project, cancellationToken);
        logger.LogInformation(
            "已轮换项目访问密钥。ProjectId={ProjectId}, ProjectKey={ProjectKey}, RotatedBy={RotatedBy}, AccessKeyHashPrefix={AccessKeyHashPrefix}",
            project.Id,
            project.Key,
            command.RotatedBy,
            accessKeyHash[..Math.Min(8, accessKeyHash.Length)]);
        return new ProjectAccessKeyRotationResult(project.Id, project.Key, accessKey);
    }
}
