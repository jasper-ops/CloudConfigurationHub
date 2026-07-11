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
}
