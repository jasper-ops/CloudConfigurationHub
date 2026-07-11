using CloudConfigurationHub.App.Endpoints;
using CloudConfigurationHub.Application.Sdk;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text;

namespace CloudConfigurationHub.Tests.App;

public sealed class SdkConfigurationEndpointTests {
    [Fact]
    public async Task GetConfigurationAsync_returns_ok_snapshot_when_access_key_is_valid() {
        var sender = new FakeSender(new PublishedConfigurationSnapshot(
            5,
            new Dictionary<string, string> {
                ["database:connectionstring"] = "server=prod"
            }));

        var result = await SdkConfigurationEndpoints.GetConfigurationAsync(
            "order-service",
            "prod",
            "secret",
            sender,
            CancellationToken.None);

        var ok = Assert.IsType<Ok<SdkConfigurationResponse>>(result.Result);
        Assert.Equal(5, ok.Value?.Version);
        Assert.Equal("server=prod", ok.Value?.Values["database:connectionstring"]);
        var query = Assert.IsType<GetPublishedConfigurationQuery>(sender.LastMessage);
        Assert.Equal("order-service", query.ProjectId);
        Assert.Equal("prod", query.EnvironmentKey);
        Assert.Equal("secret", query.AccessKey);
    }

    [Fact]
    public async Task GetConfigurationAsync_returns_unauthorized_when_snapshot_is_missing() {
        var sender = new FakeSender(null);

        var result = await SdkConfigurationEndpoints.GetConfigurationAsync(
            "order-service",
            "prod",
            "wrong",
            sender,
            CancellationToken.None);

        Assert.IsType<UnauthorizedHttpResult>(result.Result);
    }

    [Fact]
    public async Task StreamConfigurationChangesAsync_writes_sse_version_changed_event() {
        var broadcaster = new ConfigurationChangeBroadcaster();
        var httpContext = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;
        using var cancellationTokenSource = new CancellationTokenSource();

        var streamTask = SdkConfigurationEndpoints.StreamConfigurationChangesAsync(
            "order-service",
            "prod",
            broadcaster,
            httpContext.Response,
            cancellationTokenSource.Token);
        await WaitUntilAsync(() => responseBody.Length > 0, cancellationTokenSource.Token);

        await broadcaster.PublishAsync(
            new ConfigurationChangedEvent("order-service", "prod", 13),
            CancellationToken.None);
        await WaitUntilAsync(() => Encoding.UTF8.GetString(responseBody.ToArray()).Contains("\"version\":13", StringComparison.Ordinal), cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => streamTask.AsTask());
        var body = Encoding.UTF8.GetString(responseBody.ToArray());
        Assert.Equal("text/event-stream", httpContext.Response.ContentType);
        Assert.Contains("event: version-changed", body, StringComparison.Ordinal);
        Assert.Contains("\"projectId\":\"order-service\"", body, StringComparison.Ordinal);
        Assert.Contains("\"environmentKey\":\"prod\"", body, StringComparison.Ordinal);
        Assert.Contains("\"version\":13", body, StringComparison.Ordinal);
    }

    private sealed class FakeSender(PublishedConfigurationSnapshot? snapshot) : ISender {
        public object? LastMessage { get; private set; }

        public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public ValueTask<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public ValueTask<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken) {
            LastMessage = query;
            return ValueTask.FromResult((TResponse)(object?)snapshot!);
        }

        public ValueTask<object?> Send(object message, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamQuery<TResponse> query,
            CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamCommand<TResponse> command,
            CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<object> CreateStream(object message, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, CancellationToken cancellationToken) {
        while (!condition()) {
            await Task.Delay(10, cancellationToken);
        }
    }
}
