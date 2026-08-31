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
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationHubDbContext).Assembly);
    }
}
