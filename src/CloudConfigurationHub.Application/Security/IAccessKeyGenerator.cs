namespace CloudConfigurationHub.Application.Security;

/// <summary>
/// Access Key 明文生成器。
/// </summary>
public interface IAccessKeyGenerator {
    /// <summary>
    /// 生成只展示一次的项目级只读 Access Key 明文。
    /// </summary>
    /// <returns>新的 Access Key 明文。</returns>
    string Generate();
}
