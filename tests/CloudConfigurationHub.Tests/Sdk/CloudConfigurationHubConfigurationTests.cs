using CloudConfigurationHub.Sdk;
using System.Net;
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
}
