namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 某个环境下的配置草稿值。
/// </summary>
public sealed class ConfigDraftValue {
    internal ConfigDraftValue(Guid environmentId, Guid configurationId, string value) {
        EnvironmentId = environmentId;
        ConfigurationId = configurationId;
        Value = value;
    }

    /// <summary>
    /// 草稿值所属环境 ID。
    /// </summary>
    public Guid EnvironmentId { get; }

    /// <summary>
    /// 草稿值对应的配置定义 ID。
    /// </summary>
    public Guid ConfigurationId { get; }

    /// <summary>
    /// 草稿字符串值；敏感值在基础设施层持久化前加密。
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// 用新的草稿值替换当前值。
    /// </summary>
    /// <param name="value">新的草稿字符串值。</param>
    internal void ReplaceValue(string value) {
        Value = value;
    }
}
