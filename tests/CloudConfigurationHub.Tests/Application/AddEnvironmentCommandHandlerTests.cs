using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class AddEnvironmentCommandHandlerTests {
    [Fact]
    public async Task Handle_adds_environment_saves_project_and_writes_audit_log() {
        var project = Project.Create("Order Service", "order-service");
        var repository = new FakeProjectRepository(project);
        var logger = new FakeLogger<AddEnvironmentCommandHandler>();
        var handler = new AddEnvironmentCommandHandler(repository, logger);

        var result = await handler.Handle(
            new AddEnvironmentCommand(project.Id, "Production", "prod"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Production", result.Name);
        Assert.Equal("prod", result.Key);
        Assert.True(repository.WasSaved);
        Assert.Contains(project.Environments, environment => environment.Id == result.Id);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已添加项目环境", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(project.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("prod", logEntry.Message, StringComparison.Ordinal);
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
