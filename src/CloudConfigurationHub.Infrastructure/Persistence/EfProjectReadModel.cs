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
    private const string SensitiveMask = "******";

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

    /// <summary>
    /// 读取项目配置管理详情，并对敏感配置草稿值脱敏。
    /// </summary>
    /// <param name="projectId">项目 ID。</param>
    /// <param name="cancellationToken">取消令牌，用于终止数据库读取。</param>
    /// <returns>项目详情；不存在时返回 <see langword="null"/>。</returns>
    public async ValueTask<ProjectDetail?> GetProjectDetailAsync(Guid projectId, CancellationToken cancellationToken) {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Include(item => item.Environments)
            .Include(item => item.Configurations)
            .Include("_draftValues")
            .SingleOrDefaultAsync(item => item.Id == projectId, cancellationToken);
        if (project is null) {
            logger.LogWarning(
                "项目详情读取失败。ProjectId={ProjectId}, Reason={Reason}",
                projectId,
                "ProjectNotFound");
            return null;
        }

        var environments = project.Environments
            .OrderBy(item => item.Name)
            .Select(item => new EnvironmentSummary(item.Id, item.Name, item.Key))
            .ToArray();
        var configurations = project.Configurations
            .OrderBy(item => item.Group)
            .ThenBy(item => item.Key)
            .Select(configuration => new ConfigurationDetail(
                configuration.Id,
                configuration.Group,
                configuration.Key,
                configuration.IsSensitive,
                environments
                    .Select(environment => BuildEnvironmentDraftValue(project, configuration, environment))
                    .ToArray()))
            .ToArray();

        logger.LogInformation(
            "已从数据库读取项目详情。ProjectId={ProjectId}, EnvironmentCount={EnvironmentCount}, ConfigurationCount={ConfigurationCount}",
            project.Id,
            environments.Length,
            configurations.Length);
        return new ProjectDetail(project.Id, project.Name, project.Key, environments, configurations);
    }

    private static EnvironmentDraftValue BuildEnvironmentDraftValue(
        Domain.Projects.Project project,
        Domain.Projects.ConfigDefinition configuration,
        EnvironmentSummary environment) {
        var draftValue = project.DraftValues.SingleOrDefault(item =>
            item.EnvironmentId == environment.Id && item.ConfigurationId == configuration.Id);
        if (draftValue is null) {
            return new EnvironmentDraftValue(environment.Id, environment.Key, string.Empty, HasValue: false);
        }

        return new EnvironmentDraftValue(
            environment.Id,
            environment.Key,
            configuration.IsSensitive ? SensitiveMask : draftValue.Value,
            HasValue: true);
    }
}
