using CloudConfigurationHub.Domain.Projects;
using CloudConfigurationHub.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CloudConfigurationHub.Tests.Infrastructure;

public sealed class EfProjectReadModelTests {
    [Fact]
    public async Task ListProjectsAsync_returns_project_cards_ordered_by_name() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        var orderProject = Project.Create("Order Service", "order-service");
        var prodEnvironment = orderProject.AddEnvironment("Production", "prod");
        var configuration = orderProject.AddConfiguration("Database", "ConnectionString", isSensitive: false);
        orderProject.SetDraftValue(prodEnvironment.Id, configuration.Id, "server=prod");
        orderProject.PublishEnvironment(prodEnvironment.Id, "首次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        var billingProject = Project.Create("Billing Service", "billing-service");
        billingProject.AddEnvironment("Production", "prod");
        await repository.AddAsync(orderProject, CancellationToken.None);
        await repository.AddAsync(billingProject, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var readModel = new EfProjectReadModel(assertContext, NullLogger<EfProjectReadModel>.Instance);

        var projects = await readModel.ListProjectsAsync(CancellationToken.None);

        Assert.Equal(["Billing Service", "Order Service"], projects.Select(item => item.Name).ToArray());
        var orderCard = projects.Single(item => item.Key == "order-service");
        Assert.Equal(1, orderCard.EnvironmentCount);
        Assert.Equal(1, orderCard.ConfigurationCount);
        Assert.Equal(1, orderCard.ReleaseCount);
    }
}
