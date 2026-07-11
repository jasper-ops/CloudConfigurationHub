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
    private readonly CancellationTokenSource _disposeTokenSource = new();
    private readonly bool _ownsHttpClient;
    private Task? _sseListenerTask;

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
        ApplySnapshot(snapshot);
        PersistLocalCache(snapshot);
        StartSseListenerIfNeeded();
    }

    /// <summary>
    /// 释放内部 HTTP 客户端资源。
    /// </summary>
    public void Dispose() {
        _disposeTokenSource.Cancel();
        if (_ownsHttpClient) {
            _httpClient.Dispose();
        }

        _disposeTokenSource.Dispose();
    }

    private void StartSseListenerIfNeeded() {
        if (!_options.EnableSse || _sseListenerTask is not null) {
            return;
        }

        _sseListenerTask = Task.Run(() => ListenForChangesAsync(_disposeTokenSource.Token));
    }

    private async Task ListenForChangesAsync(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                await foreach (var _ in ReadSseEventsAsync(cancellationToken)) {
                    var snapshot = await LoadRemoteSnapshotAsync(cancellationToken);
                    ApplySnapshot(snapshot);
                    PersistLocalCache(snapshot);
                    OnReload();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                return;
            }
            catch (Exception) {
                await Task.Delay(_options.SseReconnectInterval, cancellationToken);
            }
        }
    }

    private async IAsyncEnumerable<ConfigurationChangedEvent> ReadSseEventsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        var endpoint = new Uri(
            _options.Endpoint,
            $"/api/sdk/v1/projects/{Uri.EscapeDataString(_options.ProjectId)}/environments/{Uri.EscapeDataString(_options.EnvironmentKey)}/configuration/stream");
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("X-CCH-Access-Key", _options.AccessKey);
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        string? eventName = null;
        var dataLines = new List<string>();

        while (!cancellationToken.IsCancellationRequested) {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) {
                break;
            }

            if (line.Length == 0) {
                if (eventName == "version-changed" && dataLines.Count > 0) {
                    var json = string.Join('\n', dataLines);
                    var changedEvent = JsonSerializer.Deserialize<ConfigurationChangedEvent>(json, JsonOptions);
                    if (changedEvent is not null) {
                        yield return changedEvent;
                    }
                }

                eventName = null;
                dataLines.Clear();
                continue;
            }

            if (line.StartsWith("event: ", StringComparison.Ordinal)) {
                eventName = line["event: ".Length..];
            }
            else if (line.StartsWith("data: ", StringComparison.Ordinal)) {
                dataLines.Add(line["data: ".Length..]);
            }
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
        return LoadRemoteSnapshotAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    private async Task<ConfigurationSnapshot> LoadRemoteSnapshotAsync(CancellationToken cancellationToken) {
        var endpoint = new Uri(
            _options.Endpoint,
            $"/api/sdk/v1/projects/{Uri.EscapeDataString(_options.ProjectId)}/environments/{Uri.EscapeDataString(_options.EnvironmentKey)}/configuration");
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("X-CCH-Access-Key", _options.AccessKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var snapshot = await response.Content.ReadFromJsonAsync<ConfigurationSnapshot>(
            JsonOptions,
            cancellationToken);
        return snapshot ?? throw new InvalidOperationException("配置中心返回空配置快照。");
    }

    private void ApplySnapshot(ConfigurationSnapshot snapshot) {
        Data = snapshot.Values.ToDictionary(
            item => item.Key,
            item => (string?)item.Value,
            StringComparer.OrdinalIgnoreCase);
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
