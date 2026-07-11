using CloudConfigurationHub.Application;
using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class PublishEnvironmentCommandHandlerTests {
    [Fact]
    public async Task Handle_publishes_environment_saves_project_broadcasts_change_and_writes_audit_log() {
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: false);
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod");
        var repository = new FakeProjectRepository(project);
        var broadcaster = new FakeConfigurationChangeBroadcaster();
        var clock = new FixedClock(DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        var logger = new FakeLogger<PublishEnvironmentCommandHandler>();
        var handler = new PublishEnvironmentCommandHandler(repository, broadcaster, clock, logger);

        var result = await handler.Handle(
            new PublishEnvironmentCommand(project.Id, environment.Id, "首次发布", "admin"),
            CancellationToken.None);

        Assert.Equal(1, result.Version);
        Assert.Equal("首次发布", result.Note);
        Assert.True(repository.WasSaved);
        Assert.Equal(new ConfigurationChangedEvent("order-service", "prod", 1), broadcaster.PublishedEvents.Single());
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已发布环境配置", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(project.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(environment.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("1", logEntry.Message, StringComparison.Ordinal);
    }

    private sealed class FakeProjectRepository(Project? project) : IProjectRepository {
        public bool WasSaved { get; private set; }

        public ValueTask AddAsync(Project project, CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public ValueTask<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken) {
            return ValueTask.FromResult(project);
        }

        public ValueTask SaveChangesAsync(Project project, CancellationToken cancellationToken) {
            WasSaved = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeConfigurationChangeBroadcaster : IConfigurationChangeBroadcaster {
        public List<ConfigurationChangedEvent> PublishedEvents { get; } = [];

        public IAsyncEnumerable<ConfigurationChangedEvent> Subscribe(
            string projectId,
            string environmentKey,
            CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        public ValueTask PublishAsync(ConfigurationChangedEvent changedEvent, CancellationToken cancellationToken) {
            PublishedEvents.Add(changedEvent);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock {
        public DateTimeOffset UtcNow => now;
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
