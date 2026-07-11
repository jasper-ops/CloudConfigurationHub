using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class CreateProjectCommandHandlerTests {
    [Fact]
    public async Task Handle_creates_project_saves_it_and_writes_audit_log() {
        var repository = new FakeProjectRepository();
        var logger = new FakeLogger<CreateProjectCommandHandler>();
        var handler = new CreateProjectCommandHandler(repository, logger);

        var result = await handler.Handle(
            new CreateProjectCommand("Order Service", "order-service"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Order Service", result.Name);
        Assert.Equal("order-service", result.Key);
        var savedProject = Assert.Single(repository.SavedProjects);
        Assert.Equal(result.Id, savedProject.Id);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已创建配置项目", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("order-service", logEntry.Message, StringComparison.Ordinal);
    }

    private sealed class FakeProjectRepository : IProjectRepository {
        public List<Project> SavedProjects { get; } = [];

        public ValueTask AddAsync(Project project, CancellationToken cancellationToken) {
            SavedProjects.Add(project);
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
