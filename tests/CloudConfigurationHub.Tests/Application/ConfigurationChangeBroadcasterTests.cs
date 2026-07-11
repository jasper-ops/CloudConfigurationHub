using CloudConfigurationHub.Application.Sdk;

namespace CloudConfigurationHub.Tests.Application;

public sealed class ConfigurationChangeBroadcasterTests {
    [Fact]
    public async Task PublishAsync_delivers_version_changed_event_to_matching_subscribers() {
        var broadcaster = new ConfigurationChangeBroadcaster();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var events = broadcaster.Subscribe("order-service", "prod", cancellationTokenSource.Token);
        var enumerator = events.GetAsyncEnumerator(cancellationTokenSource.Token);
        var pendingMove = enumerator.MoveNextAsync();

        await broadcaster.PublishAsync(
            new ConfigurationChangedEvent("order-service", "prod", 9),
            CancellationToken.None);

        Assert.True(await pendingMove);
        Assert.Equal("order-service", enumerator.Current.ProjectId);
        Assert.Equal("prod", enumerator.Current.EnvironmentKey);
        Assert.Equal(9, enumerator.Current.Version);
        await enumerator.DisposeAsync();
    }
}
