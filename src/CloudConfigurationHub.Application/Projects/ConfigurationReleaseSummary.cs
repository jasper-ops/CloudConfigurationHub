namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 配置发布版本摘要。
/// </summary>
/// <param name="Id">发布版本 ID。</param>
/// <param name="EnvironmentId">发布目标环境 ID。</param>
/// <param name="Version">环境内递增版本号。</param>
/// <param name="Note">发布备注。</param>
/// <param name="PublishedBy">发布人标识。</param>
/// <param name="PublishedAt">发布时间。</param>
/// <param name="Values">该发布版本冻结的配置值摘要集合。</param>
public sealed record ConfigurationReleaseSummary(
    Guid Id,
    Guid EnvironmentId,
    int Version,
    string Note,
    string PublishedBy,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ConfigurationReleaseValueSummary> Values);
