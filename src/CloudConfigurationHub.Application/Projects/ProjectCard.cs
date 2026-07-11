namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端项目列表卡片数据。
/// </summary>
/// <param name="Id">项目 ID。</param>
/// <param name="Name">项目显示名称。</param>
/// <param name="Key">项目唯一 Key。</param>
/// <param name="EnvironmentCount">环境数量。</param>
/// <param name="ConfigurationCount">配置定义数量。</param>
/// <param name="ReleaseCount">发布版本数量。</param>
public sealed record ProjectCard(
    Guid Id,
    string Name,
    string Key,
    int EnvironmentCount,
    int ConfigurationCount,
    int ReleaseCount);
