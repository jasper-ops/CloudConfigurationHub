using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Security;
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
    ISecretProtector secretProtector,
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
            .AsSplitQuery()
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
                project.Releases.Count,
                project.Description,
                project.CreatedAt,
                project.Environments
                    .OrderBy(environment => environment.Name)
                    .Select(environment => new EnvironmentSummary(environment.Id, environment.Name, environment.Key))
                    .ToArray()))
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
            .AsSplitQuery()
            .Include(item => item.Environments)
            .Include(item => item.Configurations)
            .Include(item => item.Releases)
            .ThenInclude(item => item.Values)
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
        var latestReleasesByEnvironmentId = project.Releases
            .GroupBy(item => item.EnvironmentId)
            .ToDictionary(
                item => item.Key,
                item => item
                    .OrderByDescending(release => release.Version)
                    .First());
        var configurations = project.Configurations
            .OrderBy(item => item.Group)
            .ThenBy(item => item.Key)
            .Select(configuration => new ConfigurationDetail(
                configuration.Id,
                configuration.Group,
                configuration.Key,
                configuration.IsSensitive,
                environments
                    .Select(environment => BuildEnvironmentDraftValue(
                        project,
                        configuration,
                        environment,
                        latestReleasesByEnvironmentId.GetValueOrDefault(environment.Id)))
                    .ToArray(),
                configuration.Description))
            .ToArray();
        var releases = project.Releases
            .OrderByDescending(item => item.PublishedAt)
            .ThenByDescending(item => item.Version)
            .Select(item => new ConfigurationReleaseSummary(
                item.Id,
                item.EnvironmentId,
                item.Version,
                item.Note,
                item.PublishedBy,
                item.PublishedAt,
                item.Values
                    .OrderBy(value => value.ConfigurationKey)
                    .Select(value => new ConfigurationReleaseValueSummary(
                        value.ConfigurationId,
                        value.ConfigurationKey,
                        value.IsSensitive ? SensitiveMask : value.Value,
                        value.IsSensitive))
                    .ToArray()))
            .ToArray();

        logger.LogInformation(
            "已从数据库读取项目详情。ProjectId={ProjectId}, EnvironmentCount={EnvironmentCount}, ConfigurationCount={ConfigurationCount}, ReleaseCount={ReleaseCount}",
            project.Id,
            environments.Length,
            configurations.Length,
            releases.Length);
        return new ProjectDetail(
            project.Id,
            project.Name,
            project.Key,
            environments,
            configurations,
            releases,
            project.Description,
            project.CreatedAt);
    }

    private EnvironmentDraftValue BuildEnvironmentDraftValue(
        Domain.Projects.Project project,
        Domain.Projects.ConfigDefinition configuration,
        EnvironmentSummary environment,
        Domain.Projects.ConfigRelease? latestRelease) {
        var draftValue = project.DraftValues.SingleOrDefault(item =>
            item.EnvironmentId == environment.Id && item.ConfigurationId == configuration.Id);
        var configurationKey = $"{configuration.Group}:{configuration.Key}";
        var publishedConfigurationKey = $"{NormalizeKey(configuration.Group)}:{NormalizeKey(configuration.Key)}";
        var latestReleaseValue = latestRelease?.Values.SingleOrDefault(item =>
            item.ConfigurationId == configuration.Id
                || item.ConfigurationKey == configurationKey
                || item.ConfigurationKey == publishedConfigurationKey);
        var publicationState = ResolvePublicationState(project, configuration, environment, draftValue, latestReleaseValue);
        var latestPublishedDisplayValue = latestReleaseValue is null
            ? string.Empty
            : configuration.IsSensitive ? SensitiveMask : latestReleaseValue.Value;
        if (draftValue is null) {
            return new EnvironmentDraftValue(
                environment.Id,
                environment.Key,
                string.Empty,
                HasValue: false,
                publicationState,
                latestPublishedDisplayValue,
                latestRelease?.Version,
                latestRelease?.PublishedAt);
        }

        return new EnvironmentDraftValue(
            environment.Id,
            environment.Key,
            configuration.IsSensitive ? SensitiveMask : draftValue.Value,
            HasValue: true,
            publicationState,
            latestPublishedDisplayValue,
            latestRelease?.Version,
            latestRelease?.PublishedAt);
    }

    private ConfigurationValuePublicationState ResolvePublicationState(
        Domain.Projects.Project project,
        Domain.Projects.ConfigDefinition configuration,
        EnvironmentSummary environment,
        Domain.Projects.ConfigDraftValue? draftValue,
        Domain.Projects.ConfigReleaseValue? latestReleaseValue) {
        var hasDraft = draftValue is not null;
        var hasLatest = latestReleaseValue is not null;
        if (!hasDraft && !hasLatest) {
            return ConfigurationValuePublicationState.NotSet;
        }

        if (hasDraft && !hasLatest) {
            return ConfigurationValuePublicationState.NotPublished;
        }

        if (!hasDraft && hasLatest) {
            return ConfigurationValuePublicationState.PendingRemoval;
        }

        return ArePersistedValuesEqual(project, configuration, environment, draftValue!, latestReleaseValue!)
            ? ConfigurationValuePublicationState.Published
            : ConfigurationValuePublicationState.PendingPublish;
    }

    private bool ArePersistedValuesEqual(
        Domain.Projects.Project project,
        Domain.Projects.ConfigDefinition configuration,
        EnvironmentSummary environment,
        Domain.Projects.ConfigDraftValue draftValue,
        Domain.Projects.ConfigReleaseValue latestReleaseValue) {
        if (!configuration.IsSensitive) {
            return string.Equals(draftValue.Value, latestReleaseValue.Value, StringComparison.Ordinal);
        }

        try {
            return string.Equals(
                secretProtector.Unprotect(draftValue.Value),
                secretProtector.Unprotect(latestReleaseValue.Value),
                StringComparison.Ordinal);
        }
        catch (Exception exception) {
            logger.LogWarning(
                exception,
                "敏感配置发布状态比较失败，已退回到持久化值比较。ProjectId={ProjectId}, EnvironmentId={EnvironmentId}, ConfigurationId={ConfigurationId}",
                project.Id,
                environment.Id,
                configuration.Id);
            return string.Equals(draftValue.Value, latestReleaseValue.Value, StringComparison.Ordinal);
        }
    }

    private static string NormalizeKey(string key) {
        return key.Trim().ToLowerInvariant();
    }
}
