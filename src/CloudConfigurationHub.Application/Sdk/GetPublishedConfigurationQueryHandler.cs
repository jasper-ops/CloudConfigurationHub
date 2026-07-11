using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Sdk;

/// <summary>
/// 处理 SDK 配置读取查询。
/// </summary>
/// <param name="reader">已发布配置读取端口。</param>
/// <param name="logger">结构化日志记录器，用于记录 SDK 读取审计事件。</param>
public sealed class GetPublishedConfigurationQueryHandler(
    IPublishedConfigurationReader reader,
    ILogger<GetPublishedConfigurationQueryHandler> logger)
    : IQueryHandler<GetPublishedConfigurationQuery, PublishedConfigurationSnapshot?> {
    /// <summary>
    /// 读取指定项目环境的最新已发布配置快照。
    /// </summary>
    /// <param name="query">SDK 配置读取查询。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>最新已发布配置快照；如果项目、环境或 Access Key 不匹配则返回 <see langword="null"/>。</returns>
    public async ValueTask<PublishedConfigurationSnapshot?> Handle(
        GetPublishedConfigurationQuery query,
        CancellationToken cancellationToken) {
        var snapshot = await reader.GetLatestAsync(
            query.ProjectId,
            query.EnvironmentKey,
            query.AccessKey,
            cancellationToken);

        if (snapshot is null) {
            logger.LogWarning(
                "SDK配置快照读取失败。ProjectId={ProjectId}, EnvironmentKey={EnvironmentKey}, Reason={Reason}",
                query.ProjectId,
                query.EnvironmentKey,
                "NotFoundOrUnauthorized");
            return null;
        }

        logger.LogInformation(
            "已读取SDK配置快照。ProjectId={ProjectId}, EnvironmentKey={EnvironmentKey}, Version={Version}, ValueCount={ValueCount}",
            query.ProjectId,
            query.EnvironmentKey,
            snapshot.Version,
            snapshot.Values.Count);
        return snapshot;
    }
}
