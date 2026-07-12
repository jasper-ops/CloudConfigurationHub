namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端项目配置详情。
/// </summary>
/// <param name="Id">项目 ID。</param>
/// <param name="Name">项目显示名称。</param>
/// <param name="Key">项目 Key。</param>
/// <param name="Description">项目说明。</param>
/// <param name="CreatedAt">项目创建时间。</param>
/// <param name="Environments">项目环境集合。</param>
/// <param name="Configurations">项目配置定义与环境草稿值集合。</param>
/// <param name="Releases">项目所有环境的发布历史摘要集合。</param>
public sealed record ProjectDetail(
    Guid Id,
    string Name,
    string Key,
    IReadOnlyList<EnvironmentSummary> Environments,
    IReadOnlyList<ConfigurationDetail> Configurations,
    IReadOnlyList<ConfigurationReleaseSummary> Releases,
    string Description = "",
    DateTimeOffset CreatedAt = default);
