using CloudConfigurationHub.Domain.Projects;

namespace CloudConfigurationHub.Tests.Domain;

public sealed class ProjectDomainTests {
    [Fact]
    public void AddEnvironment_rejects_duplicate_environment_keys_in_same_project() {
        var project = Project.Create("Order Service", "order-service");

        project.AddEnvironment("Production", "prod");

        var exception = Assert.Throws<DomainException>(() =>
            project.AddEnvironment("Production Copy", "prod"));

        Assert.Equal("项目内环境 Key 必须唯一。", exception.Message);
    }

    [Fact]
    public void AddConfiguration_rejects_duplicate_group_and_key_in_same_project() {
        var project = Project.Create("Order Service", "order-service");

        project.AddConfiguration("Database", "ConnectionString", isSensitive: true);

        var exception = Assert.Throws<DomainException>(() =>
            project.AddConfiguration("database", "connectionstring", isSensitive: false));

        Assert.Equal("项目内配置分组和 Key 组合必须唯一。", exception.Message);
    }

    [Fact]
    public void UpdateDetails_changes_project_display_fields_and_normalizes_key() {
        var project = Project.Create("Order Service", "order-service", "Old description", DateTimeOffset.Parse("2026-07-12T08:00:00Z"));

        project.UpdateDetails("Billing Service", "Billing-Service", "New description");

        Assert.Equal("Billing Service", project.Name);
        Assert.Equal("billing-service", project.Key);
        Assert.Equal("New description", project.Description);
        Assert.Equal(DateTimeOffset.Parse("2026-07-12T08:00:00Z"), project.CreatedAt);
    }

    [Fact]
    public void UpdateEnvironment_changes_display_fields_and_rejects_duplicate_key() {
        var project = Project.Create("Order Service", "order-service");
        var dev = project.AddEnvironment("Development", "dev");
        var prod = project.AddEnvironment("Production", "prod");

        var updated = project.UpdateEnvironment(dev.Id, "Testing", " Test ");

        Assert.Same(dev, updated);
        Assert.Equal("Testing", dev.Name);
        Assert.Equal("test", dev.Key);
        var exception = Assert.Throws<DomainException>(() =>
            project.UpdateEnvironment(prod.Id, "Production Copy", "test"));
        Assert.Equal("项目内环境 Key 必须唯一。", exception.Message);
    }

    [Fact]
    public void UpdateConfiguration_changes_definition_and_rejects_duplicate_key() {
        var project = Project.Create("Order Service", "order-service");
        var first = project.AddConfiguration("database", "host", isSensitive: false, description: "Host");
        var second = project.AddConfiguration("database", "port", isSensitive: false, description: "Port");

        project.UpdateConfiguration(first.Id, "redis", "url", isSensitive: true, description: "Redis URL");

        Assert.Equal("redis", first.Group);
        Assert.Equal("url", first.Key);
        Assert.True(first.IsSensitive);
        Assert.Equal("Redis URL", first.Description);
        var exception = Assert.Throws<DomainException>(() =>
            project.UpdateConfiguration(second.Id, "redis", "url", isSensitive: false, description: "Duplicate"));
        Assert.Equal("项目内配置分组和 Key 组合必须唯一。", exception.Message);
    }

    [Fact]
    public void RemoveConfiguration_deletes_definition_and_its_draft_values_without_mutating_releases() {
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Development", "dev");
        var configuration = project.AddConfiguration("database", "host", isSensitive: false, description: "Host");
        project.SetDraftValue(environment.Id, configuration.Id, "localhost");
        var release = project.PublishEnvironment(environment.Id, "发布前", "admin", DateTimeOffset.Parse("2026-07-12T08:00:00Z"));

        project.RemoveConfiguration(configuration.Id);

        Assert.Empty(project.Configurations);
        Assert.Empty(project.DraftValues);
        Assert.Equal("localhost", Assert.Single(release.Values).Value);
    }

    [Fact]
    public void PublishEnvironment_creates_immutable_snapshot_from_current_draft_values() {
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: true);
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-a");

        var release = project.PublishEnvironment(environment.Id, "首次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));

        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-b");

        var value = Assert.Single(release.Values);
        Assert.Equal("database:connectionstring", value.ConfigurationKey);
        Assert.Equal("server=prod-a", value.Value);
    }

    [Fact]
    public void RollbackEnvironment_creates_new_release_from_historical_release() {
        var project = Project.Create("Order Service", "order-service");
        var environment = project.AddEnvironment("Production", "prod");
        var configuration = project.AddConfiguration("Database", "ConnectionString", isSensitive: true);
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-a");
        var firstRelease = project.PublishEnvironment(environment.Id, "首次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:00:00Z"));
        project.SetDraftValue(environment.Id, configuration.Id, "server=prod-b");
        project.PublishEnvironment(environment.Id, "第二次发布", "admin", DateTimeOffset.Parse("2026-07-11T12:10:00Z"));

        var rollbackRelease = project.RollbackEnvironment(
            environment.Id,
            firstRelease.Id,
            "回滚到首次发布",
            "admin",
            DateTimeOffset.Parse("2026-07-11T12:20:00Z"));

        Assert.Equal(3, rollbackRelease.Version);
        Assert.Equal("server=prod-a", Assert.Single(rollbackRelease.Values).Value);
        Assert.Equal([1, 2, 3], project.Releases.Select(release => release.Version).ToArray());
    }
}
