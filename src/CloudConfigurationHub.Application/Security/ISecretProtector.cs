namespace CloudConfigurationHub.Application.Security;

/// <summary>
/// 敏感配置值保护端口。
/// </summary>
public interface ISecretProtector {
    /// <summary>
    /// 将敏感配置明文转换为可持久化的受保护文本。
    /// </summary>
    /// <param name="plainText">敏感配置明文。</param>
    /// <returns>受保护后的文本。</returns>
    string Protect(string plainText);

    /// <summary>
    /// 将受保护文本还原为敏感配置明文。
    /// </summary>
    /// <param name="protectedText">受保护文本。</param>
    /// <returns>还原后的敏感配置明文。</returns>
    string Unprotect(string protectedText);

    /// <summary>
    /// 判断给定值是否已经由当前保护器保护。
    /// </summary>
    /// <param name="value">待检查的字符串值。</param>
    /// <returns>如果值已受保护则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    bool IsProtected(string value);
}
