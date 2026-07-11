using Microsoft.Extensions.Configuration;

namespace CloudConfigurationHub.Sdk;

/// <summary>
/// CloudConfigurationHub 的 Configuration Source。
/// </summary>
/// <param name="options">SDK 连接和缓存选项。</param>
public sealed class CloudConfigurationHubConfigurationSource(CloudConfigurationHubOptions options) : IConfigurationSource {
    /// <summary>
    /// 创建实际负责加载配置的 Provider。
    /// </summary>
    /// <param name="builder">调用方的配置构建器。</param>
    /// <returns>CloudConfigurationHub 配置 Provider。</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder) {
        return new CloudConfigurationHubConfigurationProvider(options);
    }
}
