namespace CloudConfigurationHub.Infrastructure.Security;

/// <summary>
/// 配置值加密保护选项。
/// </summary>
public sealed class ConfigurationValueProtectionOptions {
    /// <summary>
    /// 用于派生敏感配置加密密钥的主密钥。
    /// </summary>
    public string MasterKey { get; set; } = string.Empty;
}
