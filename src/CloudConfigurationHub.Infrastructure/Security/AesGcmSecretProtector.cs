using System.Security.Cryptography;
using System.Text;
using CloudConfigurationHub.Application.Security;
using Microsoft.Extensions.Options;

namespace CloudConfigurationHub.Infrastructure.Security;

/// <summary>
/// 使用 AES-GCM 保护敏感配置值的实现。
/// </summary>
/// <param name="options">配置值保护选项。</param>
public sealed class AesGcmSecretProtector(IOptions<ConfigurationValueProtectionOptions> options) : ISecretProtector {
    private const string Prefix = "cch:v1:";
    private const int NonceSizeInBytes = 12;
    private const int TagSizeInBytes = 16;

    /// <summary>
    /// 加密敏感配置明文。
    /// </summary>
    /// <param name="plainText">敏感配置明文。</param>
    /// <returns>带版本前缀的密文载荷。</returns>
    public string Protect(string plainText) {
        ArgumentNullException.ThrowIfNull(plainText);
        var key = DeriveKey();
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeInBytes);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSizeInBytes];
        using var aes = new AesGcm(key, TagSizeInBytes);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);
        return $"{Prefix}{Convert.ToBase64String(payload)}";
    }

    /// <summary>
    /// 解密敏感配置密文。
    /// </summary>
    /// <param name="protectedText">带版本前缀的密文载荷。</param>
    /// <returns>解密后的敏感配置明文。</returns>
    public string Unprotect(string protectedText) {
        ArgumentNullException.ThrowIfNull(protectedText);
        if (!IsProtected(protectedText)) {
            return protectedText;
        }

        var payload = Convert.FromBase64String(protectedText[Prefix.Length..]);
        if (payload.Length < NonceSizeInBytes + TagSizeInBytes) {
            throw new CryptographicException("敏感配置密文载荷无效。");
        }

        var nonce = payload[..NonceSizeInBytes];
        var tag = payload[NonceSizeInBytes..(NonceSizeInBytes + TagSizeInBytes)];
        var cipherBytes = payload[(NonceSizeInBytes + TagSizeInBytes)..];
        var plainBytes = new byte[cipherBytes.Length];
        using var aes = new AesGcm(DeriveKey(), TagSizeInBytes);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// 判断值是否使用当前密文格式。
    /// </summary>
    /// <param name="value">待检查的字符串值。</param>
    /// <returns>如果值包含当前版本前缀则返回 <see langword="true"/>。</returns>
    public bool IsProtected(string value) {
        return value.StartsWith(Prefix, StringComparison.Ordinal);
    }

    private byte[] DeriveKey() {
        var masterKey = options.Value.MasterKey;
        if (string.IsNullOrWhiteSpace(masterKey)) {
            throw new InvalidOperationException("未配置敏感配置主密钥。请设置 ConfigurationHub:Protection:MasterKey。");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
    }
}
