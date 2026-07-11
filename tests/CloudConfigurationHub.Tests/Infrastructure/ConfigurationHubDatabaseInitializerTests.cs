using CloudConfigurationHub.Domain.Projects;
using CloudConfigurationHub.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CloudConfigurationHub.Tests.Infrastructure;

public sealed class ConfigurationHubDatabaseInitializerTests {
    [Fact]
    public async Task InitializeAsync_creates_configuration_tables_when_sqlite_database_already_exists_without_project_tables() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using (var command = connection.CreateCommand()) {
            command.CommandText = "CREATE TABLE AspNetUsers (Id TEXT NOT NULL PRIMARY KEY);";
            await command.ExecuteNonQueryAsync();
        }
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new ConfigurationHubDbContext(options);
        var initializer = new ConfigurationHubDatabaseInitializer(
            dbContext,
            NullLogger<ConfigurationHubDatabaseInitializer>.Instance);

        await initializer.InitializeAsync(CancellationToken.None);
        dbContext.Projects.Add(Project.Create("Order Service", "order-service"));
        await dbContext.SaveChangesAsync();

        Assert.Equal(1, await dbContext.Projects.CountAsync());
    }
}
