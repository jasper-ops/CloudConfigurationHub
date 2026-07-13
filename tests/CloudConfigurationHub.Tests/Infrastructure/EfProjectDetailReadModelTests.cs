using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Security;
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
        var readModel = new EfProjectReadModel(
            assertContext,
            new PassThroughSecretProtector(),
            NullLogger<EfProjectReadModel>.Instance);

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

    [Fact]
    public async Task GetProjectDetailAsync_returns_release_history_ordered_by_environment_and_version_descending() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var project = Project.Create("Order Service", "order-service");
        var production = project.AddEnvironment("Production", "prod");
        var staging = project.AddEnvironment("Staging", "staging");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: false);
        project.SetDraftValue(production.Id, configuration.Id, "server=prod-a");
        project.PublishEnvironment(production.Id, "首次发布", "alice", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        project.SetDraftValue(production.Id, configuration.Id, "server=prod-b");
        project.PublishEnvironment(production.Id, "第二次发布", "bob", DateTimeOffset.Parse("2026-07-11T12:10:00Z"));
        project.SetDraftValue(staging.Id, configuration.Id, "server=staging");
        project.PublishEnvironment(staging.Id, "预发发布", "carol", DateTimeOffset.Parse("2026-07-11T12:20:00Z"));
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var readModel = new EfProjectReadModel(
            assertContext,
            new PassThroughSecretProtector(),
            NullLogger<EfProjectReadModel>.Instance);

        var detail = await readModel.GetProjectDetailAsync(project.Id, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal(3, detail.Releases.Count);
        Assert.Equal([staging.Id, production.Id, production.Id], detail.Releases.Select(item => item.EnvironmentId).ToArray());
        Assert.Equal([1, 2, 1], detail.Releases.Select(item => item.Version).ToArray());
        Assert.Equal("第二次发布", detail.Releases.Single(item => item.EnvironmentId == production.Id && item.Version == 2).Note);
    }

    [Fact]
    public async Task GetProjectDetailAsync_returns_release_values_for_publish_diff_and_masks_sensitive_values() {
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
        project.SetDraftValue(environment.Id, feature.Id, "false");
        project.PublishEnvironment(environment.Id, "首次发布", "alice", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var readModel = new EfProjectReadModel(
            assertContext,
            new PassThroughSecretProtector(),
            NullLogger<EfProjectReadModel>.Instance);

        var detail = await readModel.GetProjectDetailAsync(project.Id, CancellationToken.None);

        Assert.NotNull(detail);
        var release = Assert.Single(detail.Releases);
        Assert.Equal(2, release.Values.Count);
        Assert.Equal("******", release.Values.Single(item => item.ConfigurationKey == "database:password").DisplayValue);
        Assert.Equal("false", release.Values.Single(item => item.ConfigurationKey == "feature:enabled").DisplayValue);
        Assert.True(release.Values.Single(item => item.ConfigurationKey == "database:password").IsSensitive);
    }

    [Fact]
    public async Task GetProjectDetailAsync_returns_publication_state_for_each_environment_value() {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ConfigurationHubDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var arrangeContext = new ConfigurationHubDbContext(options);
        await arrangeContext.Database.EnsureCreatedAsync();
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var published = project.AddConfiguration("Feature", "Published", isSensitive: false);
        var modified = project.AddConfiguration("Feature", "Modified", isSensitive: false);
        var removed = project.AddConfiguration("Feature", "Removed", isSensitive: false);
        project.SetDraftValue(environment.Id, published.Id, "same");
        project.SetDraftValue(environment.Id, modified.Id, "old");
        project.SetDraftValue(environment.Id, removed.Id, "gone");
        project.PublishEnvironment(environment.Id, "首次发布", "alice", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        project.SetDraftValue(environment.Id, modified.Id, "new");
        var notPublished = project.AddConfiguration("Feature", "NotPublished", isSensitive: false);
        project.SetDraftValue(environment.Id, notPublished.Id, "draft-only");
        project.AddConfiguration("Feature", "NotSet", isSensitive: false);
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await arrangeContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM ConfigDraftValues WHERE ConfigurationId = {0}",
            removed.Id);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var readModel = new EfProjectReadModel(
            assertContext,
            new PassThroughSecretProtector(),
            NullLogger<EfProjectReadModel>.Instance);

        var detail = await readModel.GetProjectDetailAsync(project.Id, CancellationToken.None);

        Assert.NotNull(detail);
        Assert.Equal(
            ConfigurationValuePublicationState.Published,
            ValueFor(detail, "published").PublicationState);
        Assert.Equal(
            ConfigurationValuePublicationState.PendingPublish,
            ValueFor(detail, "modified").PublicationState);
        Assert.Equal(
            ConfigurationValuePublicationState.PendingRemoval,
            ValueFor(detail, "removed").PublicationState);
        Assert.Equal(
            ConfigurationValuePublicationState.NotPublished,
            ValueFor(detail, "notpublished").PublicationState);
        Assert.Equal(
            ConfigurationValuePublicationState.NotSet,
            ValueFor(detail, "notset").PublicationState);
        Assert.Equal("old", ValueFor(detail, "modified").LatestPublishedDisplayValue);
        Assert.Equal(1, ValueFor(detail, "modified").LatestPublishedVersion);
        Assert.Equal(DateTimeOffset.Parse("2026-07-11T12:00:00Z"), ValueFor(detail, "modified").LatestPublishedAt);
    }

    [Fact]
    public async Task GetProjectDetailAsync_compares_sensitive_values_without_exposing_plain_text() {
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
        project.SetDraftValue(environment.Id, password.Id, "protected:old-secret");
        project.PublishEnvironment(environment.Id, "首次发布", "alice", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        project.SetDraftValue(environment.Id, password.Id, "protected:new-secret");
        var repository = new EfProjectRepository(arrangeContext, NullLogger<EfProjectRepository>.Instance);
        await repository.AddAsync(project, CancellationToken.None);
        await using var assertContext = new ConfigurationHubDbContext(options);
        var readModel = new EfProjectReadModel(
            assertContext,
            new PrefixSecretProtector(),
            NullLogger<EfProjectReadModel>.Instance);

        var detail = await readModel.GetProjectDetailAsync(project.Id, CancellationToken.None);

        Assert.NotNull(detail);
        var value = ValueFor(detail, "password");
        Assert.Equal(ConfigurationValuePublicationState.PendingPublish, value.PublicationState);
        Assert.Equal("******", value.DisplayValue);
        Assert.Equal("******", value.LatestPublishedDisplayValue);
        Assert.DoesNotContain("secret", value.DisplayValue, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", value.LatestPublishedDisplayValue, StringComparison.OrdinalIgnoreCase);
    }

    private static EnvironmentDraftValue ValueFor(ProjectDetail detail, string key) =>
        detail.Configurations
            .Single(item => item.Key == key)
            .Values
            .Single(item => item.EnvironmentKey == "prod");

    private sealed class PassThroughSecretProtector : ISecretProtector {
        public string Protect(string plainText) => plainText;

        public string Unprotect(string protectedText) => protectedText;

        public bool IsProtected(string value) => false;
    }

    private sealed class PrefixSecretProtector : ISecretProtector {
        public string Protect(string plainText) => $"protected:{plainText}";

        public string Unprotect(string protectedText) =>
            IsProtected(protectedText) ? protectedText["protected:".Length..] : protectedText;

        public bool IsProtected(string value) => value.StartsWith("protected:", StringComparison.Ordinal);
    }
}
