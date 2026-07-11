using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Application.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// 基于 EF Core 的已发布配置快照读取器。
/// </summary>
/// <param name="dbContext">配置中心数据库上下文。</param>
/// <param name="accessKeyHasher">Access Key 哈希器。</param>
/// <param name="secretProtector">敏感配置值保护器，用于向 SDK 返回明文配置值。</param>
/// <param name="logger">结构化日志记录器，用于记录 SDK 读取持久化侧事件。</param>
public sealed class EfPublishedConfigurationReader(
    ConfigurationHubDbContext dbContext,
    IAccessKeyHasher accessKeyHasher,
    ISecretProtector secretProtector,
    ILogger<EfPublishedConfigurationReader> logger) : IPublishedConfigurationReader {
    /// <summary>
    /// 读取指定项目环境的最新已发布配置快照。
    /// </summary>
    /// <param name="projectId">项目 ID 或项目 Key。</param>
    /// <param name="environmentKey">环境 Key。</param>
    /// <param name="accessKey">项目级只读 Access Key。</param>
    /// <param name="cancellationToken">取消令牌，用于终止数据库查询。</param>
    /// <returns>最新已发布配置快照；如果项目、环境或 Access Key 不匹配则返回 <see langword="null"/>。</returns>
    public async ValueTask<PublishedConfigurationSnapshot?> GetLatestAsync(
        string projectId,
        string environmentKey,
        string accessKey,
        CancellationToken cancellationToken) {
        var accessKeyHash = accessKeyHasher.Hash(accessKey);
        var normalizedProjectId = projectId.Trim().ToLowerInvariant();
        var normalizedEnvironmentKey = environmentKey.Trim().ToLowerInvariant();
        var project = await dbContext.Projects
            .Include(item => item.Environments)
            .Include(item => item.Releases)
            .ThenInclude(item => item.Values)
            .SingleOrDefaultAsync(
                item => item.Key == normalizedProjectId && item.AccessKeyHash == accessKeyHash,
                cancellationToken);

        if (project is null) {
            logger.LogWarning(
                "SDK配置快照数据库读取失败。ProjectId={ProjectId}, EnvironmentKey={EnvironmentKey}, Reason={Reason}",
                normalizedProjectId,
                normalizedEnvironmentKey,
                "ProjectOrAccessKeyMismatch");
            return null;
        }

        var environment = project.Environments.SingleOrDefault(item => item.Key == normalizedEnvironmentKey);
        if (environment is null) {
            logger.LogWarning(
                "SDK配置快照数据库读取失败。ProjectId={ProjectId}, EnvironmentKey={EnvironmentKey}, Reason={Reason}",
                normalizedProjectId,
                normalizedEnvironmentKey,
                "EnvironmentNotFound");
            return null;
        }

        var latestRelease = project.Releases
            .Where(item => item.EnvironmentId == environment.Id)
            .OrderByDescending(item => item.Version)
            .FirstOrDefault();
        if (latestRelease is null) {
            logger.LogWarning(
                "SDK配置快照数据库读取失败。ProjectId={ProjectId}, EnvironmentKey={EnvironmentKey}, Reason={Reason}",
                normalizedProjectId,
                normalizedEnvironmentKey,
                "ReleaseNotFound");
            return null;
        }

        logger.LogInformation(
            "已从数据库读取SDK配置快照。ProjectId={ProjectId}, EnvironmentKey={EnvironmentKey}, Version={Version}, ValueCount={ValueCount}, SensitiveValueCount={SensitiveValueCount}",
            normalizedProjectId,
            normalizedEnvironmentKey,
            latestRelease.Version,
            latestRelease.Values.Count,
            latestRelease.Values.Count(item => item.IsSensitive));
        return new PublishedConfigurationSnapshot(
            latestRelease.Version,
            latestRelease.Values.ToDictionary(item => item.ConfigurationKey, ResolveSdkValue));
    }

    private string ResolveSdkValue(Domain.Projects.ConfigReleaseValue value) {
        if (!value.IsSensitive) {
            return value.Value;
        }

        if (!secretProtector.IsProtected(value.Value)) {
            logger.LogWarning(
                "敏感配置发布值未加密，已按兼容模式返回。ConfigurationKey={ConfigurationKey}",
                value.ConfigurationKey);
            return value.Value;
        }

        return secretProtector.Unprotect(value.Value);
    }
}
