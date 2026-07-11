using CloudConfigurationHub.Sdk;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CloudConfigurationHub.Tests.Sdk;

public sealed class CloudConfigurationHubConfigurationTests {
    [Fact]
    public void AddCloudConfigurationHub_loads_remote_configuration_values() {
        using var handler = new StubHttpMessageHandler(
            """{"version":7,"values":{"database:connectionstring":"server=prod","feature:enabled":"true"}}""");
        using var cacheFile = new TemporaryFile();
        var configuration = new ConfigurationBuilder()
            .AddCloudConfigurationHub(options => {
                options.Endpoint = new Uri("https://config.local");
                options.ProjectId = "order-service";
                options.EnvironmentKey = "prod";
                options.AccessKey = "secret";
                options.LocalCachePath = cacheFile.Path;
                options.EnableSse = false;
                options.HttpMessageHandler = handler;
            })
            .Build();

        Assert.Equal("server=prod", configuration["database:connectionstring"]);
        Assert.Equal("true", configuration["feature:enabled"]);
        Assert.Single(handler.Requests);
        Assert.Equal("/api/sdk/v1/projects/order-service/environments/prod/configuration", handler.Requests[0].RequestUri?.AbsolutePath);
        Assert.True(handler.Requests[0].Headers.Contains("X-CCH-Access-Key"));
    }

    [Fact]
    public void AddCloudConfigurationHub_falls_back_to_local_cache_when_remote_is_unavailable() {
        using var handler = new StubHttpMessageHandler("service unavailable", HttpStatusCode.ServiceUnavailable);
        using var cacheFile = new TemporaryFile();
        File.WriteAllText(
            cacheFile.Path,
            """{"version":3,"values":{"database:connectionstring":"server=cached"}}""");

        var configuration = new ConfigurationBuilder()
            .AddCloudConfigurationHub(options => {
                options.Endpoint = new Uri("https://config.local");
                options.ProjectId = "order-service";
                options.EnvironmentKey = "prod";
                options.AccessKey = "secret";
                options.LocalCachePath = cacheFile.Path;
                options.EnableSse = false;
                options.HttpMessageHandler = handler;
            })
            .Build();

        Assert.Equal("server=cached", configuration["database:connectionstring"]);
    }

    [Fact]
    public async Task AddCloudConfigurationHub_refreshes_configuration_when_sse_version_changed_event_arrives() {
        using var handler = new SseRefreshHttpMessageHandler();
        using var cacheFile = new TemporaryFile();
        using var configuration = (ConfigurationRoot)new ConfigurationBuilder()
            .AddCloudConfigurationHub(options => {
                options.Endpoint = new Uri("https://config.local");
                options.ProjectId = "order-service";
                options.EnvironmentKey = "prod";
                options.AccessKey = "secret";
                options.LocalCachePath = cacheFile.Path;
                options.EnableSse = true;
                options.HttpMessageHandler = handler;
            })
            .Build();

        Assert.Equal("server=v1", configuration["database:connectionstring"]);

        await handler.WriteSseAsync(
            """
            event: version-changed
            data: {"projectId":"order-service","environmentKey":"prod","version":2}


            """);

        await WaitUntilAsync(
            () => configuration["database:connectionstring"] == "server=v2",
            CancellationToken.None);
    }

    private sealed class StubHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(statusCode) {
                Content = new StringContent(content)
            });
        }
    }

    private sealed class TemporaryFile : IDisposable {
        public TemporaryFile() {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        }

        public string Path { get; }

        public void Dispose() {
            if (File.Exists(Path)) {
                File.Delete(Path);
            }
        }
    }

    private sealed class SseRefreshHttpMessageHandler : HttpMessageHandler, IDisposable {
        private readonly BlockingStream _sseStream = new();
        private int _configurationRequestCount;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.RequestUri?.AbsolutePath.EndsWith("/configuration/stream", StringComparison.Ordinal) == true) {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StreamContent(_sseStream)
                });
            }

            _configurationRequestCount++;
            var value = _configurationRequestCount == 1 ? "server=v1" : "server=v2";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent($"{{\"version\":{_configurationRequestCount},\"values\":{{\"database:connectionstring\":\"{value}\"}}}}")
            });
        }

        public Task WriteSseAsync(string text) {
            return _sseStream.WriteTextAsync(text);
        }

        public new void Dispose() {
            _sseStream.Dispose();
        }
    }

    private sealed class BlockingStream : Stream {
        private readonly Queue<byte> _buffer = new();
        private readonly SemaphoreSlim _signal = new(0);
        private bool _disposed;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public Task WriteTextAsync(string text) {
            var bytes = Encoding.UTF8.GetBytes(text);
            lock (_buffer) {
                foreach (var item in bytes) {
                    _buffer.Enqueue(item);
                }
            }

            _signal.Release(bytes.Length);
            return Task.CompletedTask;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default) {
            await _signal.WaitAsync(cancellationToken);
            lock (_buffer) {
                if (_buffer.Count == 0) {
                    return _disposed ? 0 : 0;
                }

                var count = Math.Min(destination.Length, _buffer.Count);
                for (var index = 0; index < count; index++) {
                    destination.Span[index] = _buffer.Dequeue();
                }

                return count;
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();
        }

        public override void Flush() {
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            lock (_buffer) {
                for (var index = 0; index < count; index++) {
                    _buffer.Enqueue(buffer[offset + index]);
                }
            }

            _signal.Release(count);
        }

        protected override void Dispose(bool disposing) {
            _disposed = true;
            _signal.Release();
            base.Dispose(disposing);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, CancellationToken cancellationToken) {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        while (!condition()) {
            await Task.Delay(10, linked.Token);
        }
    }
}
