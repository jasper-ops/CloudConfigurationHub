using CloudConfigurationHub.Application.Sdk;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class GetPublishedConfigurationQueryHandlerTests {
    [Fact]
    public async Task Handle_returns_latest_published_configuration_snapshot_and_writes_audit_log() {
        var reader = new FakePublishedConfigurationReader(new PublishedConfigurationSnapshot(
            Version: 12,
            Values: new Dictionary<string, string> {
                ["database:connectionstring"] = "server=prod",
                ["feature:enabled"] = "true"
            }));
        var logger = new FakeLogger<GetPublishedConfigurationQueryHandler>();
        var handler = new GetPublishedConfigurationQueryHandler(reader, logger);

        var snapshot = await handler.Handle(
            new GetPublishedConfigurationQuery("order-service", "prod", "access-key"),
            CancellationToken.None);

        Assert.NotNull(snapshot);
        Assert.Equal(12, snapshot.Version);
        Assert.Equal("server=prod", snapshot.Values["database:connectionstring"]);
        Assert.Equal(("order-service", "prod", "access-key"), reader.LastRequest);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已读取SDK配置快照", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("order-service", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("prod", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("12", logEntry.Message, StringComparison.Ordinal);
    }

    private sealed class FakePublishedConfigurationReader(PublishedConfigurationSnapshot? snapshot)
        : IPublishedConfigurationReader {
        public (string ProjectId, string EnvironmentKey, string AccessKey)? LastRequest { get; private set; }

        public ValueTask<PublishedConfigurationSnapshot?> GetLatestAsync(
            string projectId,
            string environmentKey,
            string accessKey,
            CancellationToken cancellationToken) {
            LastRequest = (projectId, environmentKey, accessKey);
            return ValueTask.FromResult(snapshot);
        }
    }

    private sealed class FakeLogger<TCategoryName> : ILogger<TCategoryName> {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message);
}
