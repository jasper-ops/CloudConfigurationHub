using CloudConfigurationHub.Application.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// 基于 EF Core 的管理端项目读取模型。
/// </summary>
/// <param name="dbContext">配置中心数据库上下文。</param>
/// <param name="logger">结构化日志记录器，用于记录管理端读取观测事件。</param>
public sealed class EfProjectReadModel(
    ConfigurationHubDbContext dbContext,
    ILogger<EfProjectReadModel> logger) : IProjectReadModel {
    /// <summary>
    /// 读取项目卡片列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于终止数据库读取。</param>
    /// <returns>按项目名称排序的项目卡片列表。</returns>
    public async ValueTask<IReadOnlyList<ProjectCard>> ListProjectsAsync(CancellationToken cancellationToken) {
        var projects = await dbContext.Projects
            .AsNoTracking()
            .Include(project => project.Environments)
            .Include(project => project.Configurations)
            .Include(project => project.Releases)
            .OrderBy(project => project.Name)
            .Select(project => new ProjectCard(
                project.Id,
                project.Name,
                project.Key,
                project.Environments.Count,
                project.Configurations.Count,
                project.Releases.Count))
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "已从数据库读取项目列表。ProjectCount={ProjectCount}",
            projects.Count);
        return projects;
    }
}
