using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class SetDraftValueCommandHandlerTests {
    [Fact]
    public async Task Handle_sets_draft_value_saves_project_and_writes_audit_log() {
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: true);
        var repository = new FakeProjectRepository(project);
        var logger = new FakeLogger<SetDraftValueCommandHandler>();
        var handler = new SetDraftValueCommandHandler(repository, logger);

        await handler.Handle(
            new SetDraftValueCommand(project.Id, environment.Id, configuration.Id, "server=prod"),
            CancellationToken.None);

        Assert.True(repository.WasSaved);
        var release = project.PublishEnvironment(environment.Id, "验证草稿", "test", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        Assert.Equal("server=prod", Assert.Single(release.Values).Value);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已保存配置草稿值", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(project.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(environment.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(configuration.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
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
