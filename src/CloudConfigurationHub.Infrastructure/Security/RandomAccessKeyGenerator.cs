using System.Security.Cryptography;
using CloudConfigurationHub.Application.Security;

namespace CloudConfigurationHub.Infrastructure.Security;

/// <summary>
/// 基于密码学随机数的 Access Key 生成器。
/// </summary>
public sealed class RandomAccessKeyGenerator : IAccessKeyGenerator {
    private const int AccessKeyByteCount = 32;

    /// <summary>
    /// 生成带产品前缀的 URL 安全 Access Key。
    /// </summary>
    /// <returns>新的项目级只读 Access Key 明文。</returns>
    public string Generate() {
        var randomBytes = RandomNumberGenerator.GetBytes(AccessKeyByteCount);
        var encodedValue = Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return $"cch_{encodedValue}";
    }
}
