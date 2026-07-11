using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Security;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class RotateProjectAccessKeyCommandHandlerTests {
    [Fact]
    public async Task Handle_generates_access_key_hashes_and_saves_it_without_logging_plaintext() {
        var project = Project.Create("Order Service", "order-service");
        var repository = new FakeProjectRepository(project);
        var accessKeyGenerator = new FakeAccessKeyGenerator("cch_test_plain_key");
        var accessKeyHasher = new FakeAccessKeyHasher();
        var logger = new FakeLogger<RotateProjectAccessKeyCommandHandler>();
        var handler = new RotateProjectAccessKeyCommandHandler(
            repository,
            accessKeyGenerator,
            accessKeyHasher,
            logger);

        var result = await handler.Handle(
            new RotateProjectAccessKeyCommand(project.Id, "admin"),
            CancellationToken.None);

        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal("order-service", result.ProjectKey);
        Assert.Equal("cch_test_plain_key", result.AccessKey);
        Assert.Equal("hash::cch_test_plain_key", project.AccessKeyHash);
        Assert.True(repository.WasSaved);
        var logEntry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("已轮换项目访问密钥", logEntry.Message, StringComparison.Ordinal);
        Assert.Contains(project.Id.ToString(), logEntry.Message, StringComparison.Ordinal);
        Assert.Contains("admin", logEntry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("cch_test_plain_key", logEntry.Message, StringComparison.Ordinal);
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

    private sealed class FakeAccessKeyGenerator(string accessKey) : IAccessKeyGenerator {
        public string Generate() {
            return accessKey;
        }
    }

    private sealed class FakeAccessKeyHasher : IAccessKeyHasher {
        public string Hash(string accessKey) {
            return $"hash::{accessKey}";
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
