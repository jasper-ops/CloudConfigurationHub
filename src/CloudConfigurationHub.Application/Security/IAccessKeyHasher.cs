namespace CloudConfigurationHub.Application.Security;

/// <summary>
/// Access Key 哈希器。
/// </summary>
public interface IAccessKeyHasher {
    /// <summary>
    /// 计算 Access Key 的不可逆哈希值。
    /// </summary>
    /// <param name="accessKey">明文 Access Key。</param>
    /// <returns>用于持久化和比较的哈希字符串。</returns>
    string Hash(string accessKey);
}
