using CloudConfigurationHub.Domain.Projects;
using CloudConfigurationHub.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CloudConfigurationHub.Tests.Infrastructure;

public sealed class EfProjectRepositoryTests {
    [Fact]
    public async Task AddAsync_persists_project_with_environments_and_configurations_to_sqlite() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var project = Project.Create("Order Service", "order-service");
        project.AddEnvironment("Production", "prod");
        project.AddConfiguration("Database", "ConnectionString", isSensitive: true);
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);

        await repository.AddAsync(project, CancellationToken.None);

        await using var assertContext = new ConfigurationHubDbContext(options);
        var savedProject = await assertContext.Projects
            .Include(item => item.Environments)
            .Include(item => item.Configurations)
            .SingleAsync();
        Assert.Equal("Order Service", savedProject.Name);
        Assert.Equal("order-service", savedProject.Key);
        Assert.Equal("prod", Assert.Single(savedProject.Environments).Key);
        Assert.True(Assert.Single(savedProject.Configurations).IsSensitive);
    }
}
