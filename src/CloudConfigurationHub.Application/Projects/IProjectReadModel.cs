namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端项目读取模型端口。
/// </summary>
public interface IProjectReadModel {
    /// <summary>
    /// 读取项目卡片列表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>项目卡片只读列表。</returns>
    ValueTask<IReadOnlyList<ProjectCard>> ListProjectsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 读取单个项目的配置管理详情。
    /// </summary>
    /// <param name="projectId">项目 ID。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>项目详情；不存在时返回 <see langword="null"/>。</returns>
    ValueTask<ProjectDetail?> GetProjectDetailAsync(Guid projectId, CancellationToken cancellationToken);
}
