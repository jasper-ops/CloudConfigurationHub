using CloudConfigurationHub.Application.Security;
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

    [Fact]
    public async Task GetLatestAsync_returns_latest_published_snapshot_for_valid_access_key() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var hasher = new Sha256AccessKeyHasher();
        var project = Project.Create("Order Service", "order-service");
        project.ReplaceAccessKeyHash(hasher.Hash("secret"));
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: false);
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-a");
        project.PublishEnvironment(environment.Id, "首次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-b");
        project.PublishEnvironment(environment.Id, "第二次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:10:00Z"));
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var reader = new EfPublishedConfigurationReader(
            assertContext,
            hasher,
            new PassThroughSecretProtector(),
            NullLogger<EfPublishedConfigurationReader>.Instance);

        var snapshot = await reader.GetLatestAsync("order-service", "prod", "secret", CancellationToken.None);
        var unauthorizedSnapshot = await reader.GetLatestAsync("order-service", "prod", "wrong", CancellationToken.None);

        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.Version);
        Assert.Equal("server=prod-b", snapshot.Values["database:connectionstring"]);
        Assert.Null(unauthorizedSnapshot);
    }

    [Fact]
    public async Task GetLatestAsync_decrypts_sensitive_values_before_returning_sdk_snapshot() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var hasher = new Sha256AccessKeyHasher();
        var protector = new PrefixSecretProtector();
        var project = Project.Create("Order Service", "order-service");
        project.ReplaceAccessKeyHash(hasher.Hash("secret"));
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "Password", isSensitive: true);
        project.SetDraftValue(environment.Id, configuration.Id, protector.Protect("plain-password"));
        project.PublishEnvironment(environment.Id, "首次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var reader = new EfPublishedConfigurationReader(
            assertContext,
            hasher,
            protector,
            NullLogger<EfPublishedConfigurationReader>.Instance);

        var snapshot = await reader.GetLatestAsync("order-service", "prod", "secret", CancellationToken.None);

        Assert.NotNull(snapshot);
        Assert.Equal("plain-password", snapshot.Values["database:password"]);
    }

    [Fact]
    public async Task SaveChangesAsync_persists_loaded_project_mutations() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var project = Project.Create("Order Service", "order-service");
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        var loadedProject = await repository.GetByIdAsync(project.Id, CancellationToken.None)
            ?? throw new InvalidOperationException("项目未保存。");

        var environment = loadedProject.AddEnvironment("Production", "prod");
        var configuration = loadedProject.AddConfiguration("Database", "ConnectionString", isSensitive: false);
        loadedProject.SetDraftValue(environment.Id, configuration.Id, "server=prod");
        loadedProject.PublishEnvironment(environment.Id, "首次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        await repository.SaveChangesAsync(loadedProject, CancellationToken.None);

        await using var assertContext = new ConfigurationHubDbContext(options);
        var savedProject = await assertContext.Projects
            .Include(item => item.Environments)
            .Include(item => item.Configurations)
            .Include(item => item.Releases)
            .ThenInclude(item => item.Values)
            .SingleAsync(item => item.Id == project.Id);
        Assert.Equal("prod", Assert.Single(savedProject.Environments).Key);
        Assert.Equal("database", Assert.Single(savedProject.Configurations).Group);
        var release = Assert.Single(savedProject.Releases);
        Assert.Equal(1, release.Version);
        Assert.Equal("server=prod", Assert.Single(release.Values).Value);
    }

    private sealed class PassThroughSecretProtector : ISecretProtector {
        public string Protect(string plainText) {
            return plainText;
        }

        public string Unprotect(string protectedText) {
            return protectedText;
        }

        public bool IsProtected(string value) {
            return false;
        }
    }

    private sealed class PrefixSecretProtector : ISecretProtector {
        public string Protect(string plainText) {
            return $"protected::{plainText}";
        }

        public string Unprotect(string protectedText) {
            return protectedText.Replace("protected::", string.Empty, StringComparison.Ordinal);
        }

        public bool IsProtected(string value) {
            return value.StartsWith("protected::", StringComparison.Ordinal);
        }
    }
}
