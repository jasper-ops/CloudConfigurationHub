using Mediator;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 处理项目列表查询。
/// </summary>
/// <param name="readModel">项目读取模型端口。</param>
/// <param name="logger">结构化日志记录器，用于记录管理端读取观测事件。</param>
public sealed class ListProjectsQueryHandler(
    IProjectReadModel readModel,
    ILogger<ListProjectsQueryHandler> logger) : IQueryHandler<ListProjectsQuery, ProjectListResult> {
    /// <summary>
    /// 读取项目列表。
    /// </summary>
    /// <param name="query">项目列表查询。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>项目列表结果。</returns>
    public async ValueTask<ProjectListResult> Handle(ListProjectsQuery query, CancellationToken cancellationToken) {
        var projects = await readModel.ListProjectsAsync(cancellationToken);
        logger.LogInformation(
            "已读取项目列表。ProjectCount={ProjectCount}",
            projects.Count);
        return new ProjectListResult(projects);
    }
}
