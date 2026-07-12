using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Domain.Projects;
using Microsoft.Extensions.Logging;

namespace CloudConfigurationHub.Tests.Application;

public sealed class ProjectCrudCommandHandlerTests {
    [Fact]
    public async Task UpdateProject_updates_project_and_saves_changes() {
        var project = Project.Create("Order Service", "order-service");
        var repository = new FakeProjectRepository(project);
        var handler = new UpdateProjectCommandHandler(repository, new FakeLogger<UpdateProjectCommandHandler>());

        var result = await handler.Handle(
            new UpdateProjectCommand(project.Id, "Billing Service", "billing-service", "Billing API"),
            CancellationToken.None);

        Assert.Equal(project.Id, result.Id);
        Assert.Equal("Billing Service", result.Name);
        Assert.Equal("billing-service", result.Key);
        Assert.Equal("Billing API", result.Description);
        Assert.True(repository.WasSaved);
    }

    [Fact]
    public async Task DeleteProject_deletes_project_from_repository() {
        var project = Project.Create("Order Service", "order-service");
        var repository = new FakeProjectRepository(project);
        var handler = new DeleteProjectCommandHandler(repository, new FakeLogger<DeleteProjectCommandHandler>());

        await handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        Assert.Same(project, repository.DeletedProject);
    }

    [Fact]
    public async Task UpdateConfiguration_updates_definition_and_environment_values() {
        var project = Project.Create("Order Service", "order-service");
        var dev = project.AddEnvironment("Development", "dev");
        var test = project.AddEnvironment("Testing", "test");
        var configuration = project.AddConfiguration("database", "host", isSensitive: false, description: "Host");
        var repository = new FakeProjectRepository(project);
        var handler = new UpdateConfigurationCommandHandler(repository, new FakeLogger<UpdateConfigurationCommandHandler>());

        var result = await handler.Handle(
            new UpdateConfigurationCommand(
                project.Id,
                configuration.Id,
                "redis",
                "url",
                true,
                "Redis URL",
                new Dictionary<Guid, string> {
                    [dev.Id] = "redis://dev",
                    [test.Id] = "redis://test"
                }),
            CancellationToken.None);

        Assert.Equal("redis", result.Group);
        Assert.Equal("url", result.Key);
        Assert.True(result.IsSensitive);
        Assert.Equal("Redis URL", result.Description);
        Assert.Contains(project.DraftValues, item => item.EnvironmentId == dev.Id && item.Value == "redis://dev");
        Assert.Contains(project.DraftValues, item => item.EnvironmentId == test.Id && item.Value == "redis://test");
        Assert.True(repository.WasSaved);
    }

    [Fact]
    public async Task DeleteConfiguration_removes_definition_and_saves_project() {
        var project = Project.Create("Order Service", "order-service");
        var configuration = project.AddConfiguration("database", "host", isSensitive: false, description: "Host");
        var repository = new FakeProjectRepository(project);
        var handler = new DeleteConfigurationCommandHandler(repository, new FakeLogger<DeleteConfigurationCommandHandler>());

        await handler.Handle(new DeleteConfigurationCommand(project.Id, configuration.Id), CancellationToken.None);

        Assert.Empty(project.Configurations);
        Assert.True(repository.WasSaved);
    }

    private sealed class FakeProjectRepository(Project? project) : IProjectRepository {
        public bool WasSaved { get; private set; }
        public Project? DeletedProject { get; private set; }

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

        public ValueTask DeleteAsync(Project project, CancellationToken cancellationToken) {
            DeletedProject = project;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeLogger<TCategoryName> : ILogger<TCategoryName> {
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
        }
    }
}
