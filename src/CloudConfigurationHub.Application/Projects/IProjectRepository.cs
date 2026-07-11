using CloudConfigurationHub.Domain.Projects;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 项目聚合的仓储端口。
/// </summary>
/// <remarks>
/// 应用层只依赖该端口，不直接依赖 EF Core 或 SQLite，以保持 DDD 分层边界。
/// </remarks>
public interface IProjectRepository {
    /// <summary>
    /// 保存一个新创建的项目聚合。
    /// </summary>
    /// <param name="project">待保存的项目聚合。</param>
    /// <param name="cancellationToken">取消令牌，用于终止持久化操作。</param>
    /// <returns>表示异步保存操作的值任务。</returns>
    ValueTask AddAsync(Project project, CancellationToken cancellationToken);

    /// <summary>
    /// 按项目 ID 读取项目聚合。
    /// </summary>
    /// <param name="projectId">项目聚合 ID。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>项目聚合；不存在时返回 <see langword="null"/>。</returns>
    ValueTask<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>
    /// 保存项目聚合上的变更。
    /// </summary>
    /// <param name="project">已发生领域行为的项目聚合。</param>
    /// <param name="cancellationToken">取消令牌，用于终止持久化操作。</param>
    /// <returns>表示异步保存操作的值任务。</returns>
    ValueTask SaveChangesAsync(Project project, CancellationToken cancellationToken);
}
