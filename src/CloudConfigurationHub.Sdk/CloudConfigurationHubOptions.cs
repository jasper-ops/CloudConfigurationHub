namespace CloudConfigurationHub.Sdk;

/// <summary>
/// CloudConfigurationHub SDK 连接、认证、缓存和刷新选项。
/// </summary>
public sealed class CloudConfigurationHubOptions {
    /// <summary>
    /// 配置中心服务端地址。
    /// </summary>
    public Uri Endpoint { get; set; } = null!;

    /// <summary>
    /// 项目唯一 Key 或 ID。
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// 环境 Key。
    /// </summary>
    public string EnvironmentKey { get; set; } = string.Empty;

    /// <summary>
    /// 项目级只读 Access Key。
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 本地 JSON 缓存文件路径；为空时不落盘缓存。
    /// </summary>
    public string? LocalCachePath { get; set; }

    /// <summary>
    /// 是否启用 SSE 实时刷新。
    /// </summary>
    public bool EnableSse { get; set; } = true;

    /// <summary>
    /// SSE 断线后的重连间隔。
    /// </summary>
    public TimeSpan SseReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 可选 HTTP 消息处理器，主要用于测试或自定义网络管道。
    /// </summary>
    public HttpMessageHandler? HttpMessageHandler { get; set; }

    /// <summary>
    /// 校验 SDK 必需连接参数是否完整。
    /// </summary>
    /// <exception cref="InvalidOperationException">当必需参数缺失时抛出。</exception>
    public void Validate() {
        if (Endpoint is null) {
            throw new InvalidOperationException("必须配置配置中心服务端地址。");
        }

        if (string.IsNullOrWhiteSpace(ProjectId)) {
            throw new InvalidOperationException("必须配置项目 ID。");
        }

        if (string.IsNullOrWhiteSpace(EnvironmentKey)) {
            throw new InvalidOperationException("必须配置环境 Key。");
        }

        if (string.IsNullOrWhiteSpace(AccessKey)) {
            throw new InvalidOperationException("必须配置 Access Key。");
        }
    }
}
