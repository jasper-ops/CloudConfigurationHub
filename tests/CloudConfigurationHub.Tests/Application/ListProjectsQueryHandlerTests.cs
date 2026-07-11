using CloudConfigurationHub.Application.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class ListProjectsQueryHandlerTests {
    [Fact]
    public async Task Handle_returns_project_cards_and_writes_observability_log() {
        var reader = new FakeProjectReadModel(new[] {
            new ProjectCard(Guid.NewGuid(), "Order Service", "order-service", 2, 5, 3),
            new ProjectCard(Guid.NewGuid(), "Billing Service", "billing-service", 1, 2, 1)
        });
        var logger = new FakeLogger<ListProjectsQueryHandler>();
        var handler = new ListProjectsQueryHandler(reader, logger);

        var result = await handler.Handle(new ListProjectsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Projects.Count);
        Assert.Equal("Order Service", result.Projects[0].Name);
        Assert.Equal(2, result.Projects[0].EnvironmentCount);
        Assert.Equal(5, result.Projects[0].ConfigurationCount);
        Assert.Equal(3, result.Projects[0].ReleaseCount);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已读取项目列表", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("2", logEntry.Message, StringComparison.Ordinal);
    }

    private sealed class FakeProjectReadModel(IReadOnlyList<ProjectCard> projects) : IProjectReadModel {
        public ValueTask<IReadOnlyList<ProjectCard>> ListProjectsAsync(CancellationToken cancellationToken) {
            return ValueTask.FromResult(projects);
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
