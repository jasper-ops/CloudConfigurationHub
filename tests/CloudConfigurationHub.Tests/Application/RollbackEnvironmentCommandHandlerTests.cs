using CloudConfigurationHub.Application;
using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class RollbackEnvironmentCommandHandlerTests {
    [Fact]
    public async Task Handle_creates_new_release_from_history_saves_broadcasts_and_logs() {
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: false);
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-a");
        var firstRelease = project.PublishEnvironment(
            environment.Id,
            "首次发布",
            "admin",
            DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-b");
        project.PublishEnvironment(
            environment.Id,
            "第二次发布",
            "admin",
            DateTimeOffset.Parse("2026-07-11T12:10:00Z"));
        var repository = new FakeProjectRepository(project);
        var broadcaster = new FakeConfigurationChangeBroadcaster();
        var clock = new FixedClock(DateTimeOffset.Parse("2026-07-11T12:20:00Z"));
        var logger = new FakeLogger<RollbackEnvironmentCommandHandler>();
        var handler = new RollbackEnvironmentCommandHandler(repository, broadcaster, clock, logger);

        var result = await handler.Handle(
            new RollbackEnvironmentCommand(project.Id, environment.Id, firstRelease.Id, "回滚到首次发布", "admin"),
            CancellationToken.None);

        Assert.Equal(3, result.Version);
        Assert.Equal("回滚到首次发布", result.Note);
        Assert.True(repository.WasSaved);
        Assert.Equal(new ConfigurationChangedEvent("order-service", "prod", 3), broadcaster.PublishedEvents.Single());
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已回滚环境配置", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(firstRelease.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("3", logEntry.Message, StringComparison.Ordinal);
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
