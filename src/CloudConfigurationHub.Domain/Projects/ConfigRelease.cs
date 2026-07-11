namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 某个项目环境的一次不可变配置发布版本。
/// </summary>
public sealed class ConfigRelease {
    private readonly List<ConfigReleaseValue> _values = [];

    private ConfigRelease() {
        Note = string.Empty;
        PublishedBy = string.Empty;
    }

    internal ConfigRelease(
        Guid id,
        Guid environmentId,
        int version,
        string note,
        string publishedBy,
        DateTimeOffset publishedAt,
        IReadOnlyCollection<ConfigReleaseValue> values) {
        Id = id;
        EnvironmentId = environmentId;
        Version = version;
        Note = note;
        PublishedBy = publishedBy;
        PublishedAt = publishedAt;
        _values.AddRange(values);
    }

    /// <summary>
    /// 发布版本 ID。
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 发布版本所属环境 ID。
    /// </summary>
    public Guid EnvironmentId { get; }

    /// <summary>
    /// 环境内递增版本号。
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// 发布备注，用于审计和回滚判断。
    /// </summary>
    public string Note { get; }

    /// <summary>
    /// 执行发布的管理员标识。
    /// </summary>
    public string PublishedBy { get; }

    /// <summary>
    /// 发布时间。
    /// </summary>
    public DateTimeOffset PublishedAt { get; }

    /// <summary>
    /// 发布时冻结的配置值快照。
    /// </summary>
    public IReadOnlyCollection<ConfigReleaseValue> Values => _values.AsReadOnly();
}
