namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 项目下的一个运行环境。
/// </summary>
public sealed class ProjectEnvironment {
    private ProjectEnvironment() {
        Name = string.Empty;
        Key = string.Empty;
    }

    internal ProjectEnvironment(Guid id, string name, string key) {
        Id = id;
        Name = name;
        Key = key;
    }

    /// <summary>
    /// 环境 ID。
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 环境显示名称。
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 环境唯一 Key，用于 SDK 读取和发布粒度定位。
    /// </summary>
    public string Key { get; private set; }

    internal void Update(string name, string key) {
        Name = name.Trim();
        Key = key.Trim().ToLowerInvariant();
    }
}
