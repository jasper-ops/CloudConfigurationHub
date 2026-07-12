namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 项目内统一的配置定义。
/// </summary>
public sealed class ConfigDefinition {
    private ConfigDefinition() {
        Group = string.Empty;
        Key = string.Empty;
        Description = string.Empty;
    }

    internal ConfigDefinition(Guid id, string group, string key, bool isSensitive, string description) {
        Id = id;
        Group = group;
        Key = key;
        IsSensitive = isSensitive;
        Description = description;
    }

    /// <summary>
    /// 配置定义 ID。
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 配置分组，发布给 SDK 时会作为配置键前缀。
    /// </summary>
    public string Group { get; private set; }

    /// <summary>
    /// 配置 Key，和分组一起在项目内唯一。
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// 指示该配置是否为敏感配置。
    /// </summary>
    public bool IsSensitive { get; private set; }

    /// <summary>
    /// 配置说明，用于管理端识别配置用途。
    /// </summary>
    public string Description { get; private set; }

    internal void Update(string group, string key, bool isSensitive, string description) {
        Group = group;
        Key = key;
        IsSensitive = isSensitive;
        Description = description;
    }
}
