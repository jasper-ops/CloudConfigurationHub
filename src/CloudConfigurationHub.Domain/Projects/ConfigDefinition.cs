namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 项目内统一的配置定义。
/// </summary>
public sealed class ConfigDefinition {
    internal ConfigDefinition(Guid id, string group, string key, bool isSensitive) {
        Id = id;
        Group = group;
        Key = key;
        IsSensitive = isSensitive;
    }

    /// <summary>
    /// 配置定义 ID。
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 配置分组，发布给 SDK 时会作为配置键前缀。
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// 配置 Key，和分组一起在项目内唯一。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 指示该配置是否为敏感配置。
    /// </summary>
    public bool IsSensitive { get; }
}
