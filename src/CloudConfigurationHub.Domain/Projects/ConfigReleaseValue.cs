namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 发布版本中的单个配置值快照。
/// </summary>
public sealed class ConfigReleaseValue {
    internal ConfigReleaseValue(Guid configurationId, string configurationKey, string value, bool isSensitive) {
        ConfigurationId = configurationId;
        ConfigurationKey = configurationKey;
        Value = value;
        IsSensitive = isSensitive;
    }

    /// <summary>
    /// 对应配置定义 ID。
    /// </summary>
    public Guid ConfigurationId { get; }

    /// <summary>
    /// SDK 可见配置键，格式为 <c>Group:Key</c>。
    /// </summary>
    public string ConfigurationKey { get; }

    /// <summary>
    /// 发布时冻结的字符串值。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 指示该值是否来自敏感配置定义。
    /// </summary>
    public bool IsSensitive { get; }
}
