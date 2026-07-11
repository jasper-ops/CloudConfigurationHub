using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CloudConfigurationHub.Sdk;

/// <summary>
/// 从 CloudConfigurationHub 服务端加载配置快照的 Configuration Provider。
/// </summary>
public sealed class CloudConfigurationHubConfigurationProvider : ConfigurationProvider, IDisposable {
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly CloudConfigurationHubOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// 创建配置 Provider。
    /// </summary>
    /// <param name="options">SDK 连接和缓存选项。</param>
    public CloudConfigurationHubConfigurationProvider(CloudConfigurationHubOptions options) {
        _options = options;
        var handler = options.HttpMessageHandler;
        _httpClient = handler is null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        _ownsHttpClient = true;
    }

    /// <summary>
    /// 从远端加载配置快照并写入本地缓存。
    /// </summary>
    public override void Load() {
        var snapshot = LoadSnapshotWithFallback();
        Data = snapshot.Values.ToDictionary(
            item => item.Key,
            item => (string?)item.Value,
            StringComparer.OrdinalIgnoreCase);
        PersistLocalCache(snapshot);
    }

    /// <summary>
    /// 释放内部 HTTP 客户端资源。
    /// </summary>
    public void Dispose() {
        if (_ownsHttpClient) {
            _httpClient.Dispose();
        }
    }

    private ConfigurationSnapshot LoadSnapshotWithFallback() {
        try {
            return LoadRemoteSnapshot();
        }
        catch (Exception) when (TryLoadLocalCache(out var snapshot)) {
            return snapshot;
        }
    }

    private ConfigurationSnapshot LoadRemoteSnapshot() {
        var endpoint = new Uri(
            _options.Endpoint,
            $"/api/sdk/v1/projects/{Uri.EscapeDataString(_options.ProjectId)}/environments/{Uri.EscapeDataString(_options.EnvironmentKey)}/configuration");
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("X-CCH-Access-Key", _options.AccessKey);
        using var response = _httpClient.SendAsync(request, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        response.EnsureSuccessStatusCode();
        var snapshot = response.Content.ReadFromJsonAsync<ConfigurationSnapshot>(cancellationToken: CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        return snapshot ?? throw new InvalidOperationException("配置中心返回空配置快照。");
    }

    private bool TryLoadLocalCache(out ConfigurationSnapshot snapshot) {
        snapshot = new ConfigurationSnapshot(0, []);
        if (string.IsNullOrWhiteSpace(_options.LocalCachePath) || !File.Exists(_options.LocalCachePath)) {
            return false;
        }

        var json = File.ReadAllText(_options.LocalCachePath);
        var cachedSnapshot = JsonSerializer.Deserialize<ConfigurationSnapshot>(json, JsonOptions);
        if (cachedSnapshot is null) {
            return false;
        }

        snapshot = cachedSnapshot;
        return true;
    }

    private void PersistLocalCache(ConfigurationSnapshot snapshot) {
        if (string.IsNullOrWhiteSpace(_options.LocalCachePath)) {
            return;
        }

        var directory = Path.GetDirectoryName(_options.LocalCachePath);
        if (!string.IsNullOrWhiteSpace(directory)) {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_options.LocalCachePath, JsonSerializer.Serialize(snapshot, JsonOptions));
    }
}
