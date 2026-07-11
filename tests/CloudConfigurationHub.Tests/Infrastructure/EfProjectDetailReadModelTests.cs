using CloudConfigurationHub.Domain.Projects;
using CloudConfigurationHub.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CloudConfigurationHub.Tests.Infrastructure;

public sealed class EfProjectDetailReadModelTests {
    [Fact]
    public async Task GetProjectDetailAsync_masks_sensitive_draft_values_and_returns_plain_non_sensitive_values() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var password = project.AddConfiguration("Database", "Password", isSensitive: true);
        var feature = project.AddConfiguration("Feature", "Enabled", isSensitive: false);
        project.SetDraftValue(environment.Id, password.Id, "cch:v1:encrypted-password");
        project.SetDraftValue(environment.Id, feature.Id, "true");
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var readModel = new EfProjectReadModel(assertContext, NullLogger<EfProjectReadModel>.Instance);

        var detail = await readModel.GetProjectDetailAsync(project.Id, CancellationToken.None);

        Assert.NotNull(detail);
        var passwordValue = detail.Configurations
            .Single(item => item.Key == "password")
            .Values
            .Single(item => item.EnvironmentKey == "prod");
        var featureValue = detail.Configurations
            .Single(item => item.Key == "enabled")
            .Values
            .Single(item => item.EnvironmentKey == "prod");
        Assert.Equal("******", passwordValue.DisplayValue);
        Assert.Equal("true", featureValue.DisplayValue);
        Assert.True(passwordValue.HasValue);
        Assert.True(featureValue.HasValue);
    }
}
