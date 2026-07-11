using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理项目配置详情查询。
/// </summary>
/// <param name="readModel">项目读取模型端口。</param>
/// <param name="logger">结构化日志记录器，用于记录管理端读取观测事件。</param>
public sealed class GetProjectDetailQueryHandler(
    IProjectReadModel readModel,
    ILogger<GetProjectDetailQueryHandler> logger) : IQueryHandler<GetProjectDetailQuery, ProjectDetail?> {
    /// <summary>
    /// 读取项目配置详情。
    /// </summary>
    /// <param name="query">项目配置详情查询。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>项目配置详情；不存在时返回 <see langword="null"/>。</returns>
    public async ValueTask<ProjectDetail?> Handle(GetProjectDetailQuery query, CancellationToken cancellationToken) {
        var detail = await readModel.GetProjectDetailAsync(query.ProjectId, cancellationToken);
        logger.LogInformation(
            "已读取项目详情。ProjectId={ProjectId}, Found={Found}, ConfigurationCount={ConfigurationCount}",
            query.ProjectId,
            detail is not null,
            detail?.Configurations.Count ?? 0);
        return detail;
    }
}
