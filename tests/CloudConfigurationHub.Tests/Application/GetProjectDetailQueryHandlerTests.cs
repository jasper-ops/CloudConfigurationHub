using CloudConfigurationHub.Application.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class GetProjectDetailQueryHandlerTests {
    [Fact]
    public async Task Handle_returns_project_detail_and_writes_observability_log() {
        var projectId = Guid.NewGuid();
        var readModel = new FakeProjectReadModel(new ProjectDetail(
            projectId,
            "Order Service",
            "order-service",
            [new EnvironmentSummary(Guid.NewGuid(), "Production", "prod")],
            [new ConfigurationDetail(
                Guid.NewGuid(),
                "database",
                "password",
                IsSensitive: true,
                [new EnvironmentDraftValue(Guid.NewGuid(), "prod", "******", HasValue: true)])]));
        var logger = new FakeLogger<GetProjectDetailQueryHandler>();
        var handler = new GetProjectDetailQueryHandler(readModel, logger);

        var result = await handler.Handle(new GetProjectDetailQuery(projectId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Order Service", result.Name);
        Assert.Equal("******", Assert.Single(Assert.Single(result.Configurations).Values).DisplayValue);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已读取项目详情", logEntry.Message, StringComparison.Ordinal);
    }

    private sealed class FakeProjectReadModel(ProjectDetail? detail) : IProjectReadModel {
        public ValueTask<IReadOnlyList<ProjectCard>> ListProjectsAsync(CancellationToken cancellationToken) {
            return ValueTask.FromResult<IReadOnlyList<ProjectCard>>([]);
        }

        public ValueTask<ProjectDetail?> GetProjectDetailAsync(Guid projectId, CancellationToken cancellationToken) {
            return ValueTask.FromResult(detail);
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
