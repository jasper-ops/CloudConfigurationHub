using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// CloudConfigurationHub 的 EF Core 数据库上下文。
/// </summary>
/// <param name="options">EF Core 上下文选项，生产环境使用 SQLite，测试环境使用 SQLite in-memory。</param>
public sealed class ConfigurationHubDbContext(DbContextOptions<ConfigurationHubDbContext> options)
    : DbContext(options) {
    /// <summary>
    /// 配置项目聚合根集合。
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// 配置 EF Core 模型映射。
    /// </summary>
    /// <param name="modelBuilder">EF Core 模型构建器。</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Project>(builder => {
            builder.ToTable("Projects");
            builder.HasKey(project => project.Id);
            builder.Property(project => project.Name).HasMaxLength(200).IsRequired();
            builder.Property(project => project.Key).HasMaxLength(100).IsRequired();
            builder.HasIndex(project => project.Key).IsUnique();
            builder.Navigation(project => project.Environments).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(project => project.Configurations).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.Navigation(project => project.Releases).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(project => project.Environments, environments => {
                environments.ToTable("ProjectEnvironments");
                environments.WithOwner().HasForeignKey("ProjectId");
                environments.HasKey("Id");
                environments.Property(environment => environment.Id).ValueGeneratedNever();
                environments.Property(environment => environment.Name).HasMaxLength(200).IsRequired();
                environments.Property(environment => environment.Key).HasMaxLength(100).IsRequired();
                environments.HasIndex("ProjectId", nameof(ProjectEnvironment.Key)).IsUnique();
            });

            builder.OwnsMany(project => project.Configurations, configurations => {
                configurations.ToTable("ConfigDefinitions");
                configurations.WithOwner().HasForeignKey("ProjectId");
                configurations.HasKey("Id");
                configurations.Property(configuration => configuration.Id).ValueGeneratedNever();
                configurations.Property(configuration => configuration.Group).HasMaxLength(100).IsRequired();
                configurations.Property(configuration => configuration.Key).HasMaxLength(200).IsRequired();
                configurations.Property(configuration => configuration.IsSensitive).IsRequired();
                configurations.HasIndex("ProjectId", nameof(ConfigDefinition.Group), nameof(ConfigDefinition.Key)).IsUnique();
            });

            builder.OwnsMany<ConfigDraftValue>("_draftValues", draftValues => {
                draftValues.ToTable("ConfigDraftValues");
                draftValues.WithOwner().HasForeignKey("ProjectId");
                draftValues.HasKey("ProjectId", nameof(ConfigDraftValue.EnvironmentId), nameof(ConfigDraftValue.ConfigurationId));
                draftValues.Property(draftValue => draftValue.Value).IsRequired();
            });

            builder.OwnsMany(project => project.Releases, releases => {
                releases.ToTable("ConfigReleases");
                releases.WithOwner().HasForeignKey("ProjectId");
                releases.HasKey(release => release.Id);
                releases.Property(release => release.Id).ValueGeneratedNever();
                releases.Property(release => release.Version).IsRequired();
                releases.Property(release => release.Note).HasMaxLength(500).IsRequired();
                releases.Property(release => release.PublishedBy).HasMaxLength(200).IsRequired();
                releases.Property(release => release.PublishedAt).IsRequired();
                releases.HasIndex("ProjectId", nameof(ConfigRelease.EnvironmentId), nameof(ConfigRelease.Version)).IsUnique();

                releases.OwnsMany(release => release.Values, values => {
                    values.ToTable("ConfigReleaseValues");
                    values.WithOwner().HasForeignKey("ReleaseId");
                    values.Property<Guid>("Id");
                    values.HasKey("Id");
                    values.Property(value => value.ConfigurationKey).HasMaxLength(400).IsRequired();
                    values.Property(value => value.Value).IsRequired();
                    values.Property(value => value.IsSensitive).IsRequired();
                });
            });
        });
    }
}
