using CloudConfigurationHub.App.Endpoints;
using CloudConfigurationHub.Application.Sdk;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

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
}
