using Microsoft.Extensions.Configuration;

namespace CloudConfigurationHub.Sdk;

/// <summary>
/// 为 <see cref="IConfigurationBuilder"/> 提供 CloudConfigurationHub 配置源扩展。
/// </summary>
public static class CloudConfigurationHubConfigurationBuilderExtensions {
    /// <summary>
    /// 添加 CloudConfigurationHub 配置源。
    /// </summary>
    /// <param name="builder">应用配置构建器。</param>
    /// <param name="configure">配置 SDK 连接参数的委托。</param>
    /// <returns>传入的配置构建器，便于继续链式调用。</returns>
    public static IConfigurationBuilder AddCloudConfigurationHub(
        this IConfigurationBuilder builder,
        Action<CloudConfigurationHubOptions> configure) {
        var options = new CloudConfigurationHubOptions();
        configure(options);
        options.Validate();
        return builder.Add(new CloudConfigurationHubConfigurationSource(options));
    }
}
