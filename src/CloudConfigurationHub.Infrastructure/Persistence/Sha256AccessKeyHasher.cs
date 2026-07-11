using System.Security.Cryptography;
using System.Text;

namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// 基于 SHA-256 的 Access Key 哈希器。
/// </summary>
public sealed class Sha256AccessKeyHasher : IAccessKeyHasher {
    /// <summary>
    /// 计算 Access Key 的 SHA-256 十六进制哈希值。
    /// </summary>
    /// <param name="accessKey">明文 Access Key。</param>
    /// <returns>小写十六进制哈希字符串。</returns>
    public string Hash(string accessKey) {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(accessKey));
        return Convert.ToHexStringLower(bytes);
    }
}
