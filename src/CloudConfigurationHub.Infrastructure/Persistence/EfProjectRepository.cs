using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// 基于 EF Core 的项目仓储实现。
/// </summary>
/// <param name="dbContext">配置中心数据库上下文。</param>
/// <param name="logger">结构化日志记录器，用于记录持久化审计事件。</param>
public sealed class EfProjectRepository(
    ConfigurationHubDbContext dbContext,
    ILogger<EfProjectRepository> logger) : IProjectRepository {
    /// <summary>
    /// 保存项目聚合并立即提交事务。
    /// </summary>
    /// <param name="project">待保存的项目聚合。</param>
    /// <param name="cancellationToken">取消令牌，用于终止数据库写入。</param>
    /// <returns>表示异步保存操作的值任务。</returns>
    public async ValueTask AddAsync(Project project, CancellationToken cancellationToken) {
        await dbContext.Projects.AddAsync(project, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "已持久化配置项目。ProjectId={ProjectId}, ProjectKey={ProjectKey}, EnvironmentCount={EnvironmentCount}, ConfigurationCount={ConfigurationCount}",
            project.Id,
            project.Key,
            project.Environments.Count,
            project.Configurations.Count);
    }

    /// <summary>
    /// 按项目 ID 读取项目聚合及其配置管理子对象。
    /// </summary>
    /// <param name="projectId">项目聚合 ID。</param>
    /// <param name="cancellationToken">取消令牌，用于终止数据库读取。</param>
    /// <returns>项目聚合；不存在时返回 <see langword="null"/>。</returns>
    public async ValueTask<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken) {
        return await dbContext.Projects
            .Include(project => project.Environments)
            .Include(project => project.Configurations)
            .Include(project => project.Releases)
            .ThenInclude(release => release.Values)
            .SingleOrDefaultAsync(project => project.Id == projectId, cancellationToken);
    }

    /// <summary>
    /// 保存项目聚合上的变更。
    /// </summary>
    /// <param name="project">已发生领域行为的项目聚合。</param>
    /// <param name="cancellationToken">取消令牌，用于终止数据库写入。</param>
    /// <returns>表示异步保存操作的值任务。</returns>
    public async ValueTask SaveChangesAsync(Project project, CancellationToken cancellationToken) {
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "已保存配置项目变更。ProjectId={ProjectId}, ProjectKey={ProjectKey}, EnvironmentCount={EnvironmentCount}, ConfigurationCount={ConfigurationCount}, ReleaseCount={ReleaseCount}",
            project.Id,
            project.Key,
            project.Environments.Count,
            project.Configurations.Count,
            project.Releases.Count);
    }
}
