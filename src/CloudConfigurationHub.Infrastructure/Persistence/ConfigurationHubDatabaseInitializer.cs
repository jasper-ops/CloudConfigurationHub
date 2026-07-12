using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// 基于 EF Core 关系型数据库创建器的配置中心数据库结构初始化器。
/// </summary>
/// <param name="dbContext">配置中心数据库上下文。</param>
/// <param name="logger">结构化日志记录器，用于记录数据库初始化过程。</param>
public sealed class ConfigurationHubDatabaseInitializer(
    ConfigurationHubDbContext dbContext,
    ILogger<ConfigurationHubDatabaseInitializer> logger) : IConfigurationHubDatabaseInitializer {
    /// <summary>
    /// 确保配置中心表已经创建；当数据库已存在但缺少配置中心表时补建模型表。
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于终止数据库初始化。</param>
    /// <returns>表示数据库初始化过程的异步任务。</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken) {
        var creator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync(cancellationToken)) {
            logger.LogInformation("配置中心数据库不存在，开始创建数据库和业务表。");
            await creator.CreateAsync(cancellationToken);
            await creator.CreateTablesAsync(cancellationToken);
            return;
        }

        if (await ProjectsTableExistsAsync(cancellationToken)) {
            await EnsureCompatibilityColumnsAsync(cancellationToken);
            logger.LogInformation("配置中心数据库表已存在，跳过初始化。");
            return;
        }

        logger.LogInformation("配置中心数据库已存在但缺少业务表，开始创建业务表。");
        await creator.CreateTablesAsync(cancellationToken);
    }

    private async Task<bool> ProjectsTableExistsAsync(CancellationToken cancellationToken) {
        if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite") {
            return await SqliteTableExistsAsync(cancellationToken);
        }

        try {
            await dbContext.Projects.AsNoTracking().AnyAsync(cancellationToken);
            return true;
        }
        catch (Exception exception) when (IsMissingTableException(exception)) {
            logger.LogWarning(exception, "配置中心 Projects 表不存在，将执行业务表初始化。");
            return false;
        }
    }

    private static bool IsMissingTableException(Exception exception) {
        var message = exception.ToString();
        return message.Contains("no such table", StringComparison.OrdinalIgnoreCase)
            || message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<bool> SqliteTableExistsAsync(CancellationToken cancellationToken) {
        var connection = dbContext.Database.GetDbConnection();
        await OpenConnectionIfNeededAsync(connection, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = 'Projects';";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        var exists = Convert.ToInt32(result) > 0;
        logger.LogInformation(
            "已检查 SQLite 配置中心表状态。ProjectsTableExists={ProjectsTableExists}",
            exists);
        return exists;
    }

    private async Task EnsureCompatibilityColumnsAsync(CancellationToken cancellationToken) {
        if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite") {
            return;
        }

        var connection = dbContext.Database.GetDbConnection();
        await OpenConnectionIfNeededAsync(connection, cancellationToken);
        await EnsureSqliteColumnAsync(
            connection,
            "Projects",
            "Description",
            "ALTER TABLE Projects ADD COLUMN Description TEXT NOT NULL DEFAULT '';",
            cancellationToken);
        await EnsureSqliteColumnAsync(
            connection,
            "Projects",
            "CreatedAt",
            "ALTER TABLE Projects ADD COLUMN CreatedAt TEXT NOT NULL DEFAULT '2026-07-12 00:00:00+08:00';",
            cancellationToken);
        await EnsureSqliteColumnAsync(
            connection,
            "ConfigDefinitions",
            "Description",
            "ALTER TABLE ConfigDefinitions ADD COLUMN Description TEXT NOT NULL DEFAULT '';",
            cancellationToken);
        await using var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = "UPDATE Projects SET CreatedAt = '2026-07-12 00:00:00+08:00' WHERE CreatedAt LIKE '1970-01-01%';";
        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureSqliteColumnAsync(
        DbConnection connection,
        string tableName,
        string columnName,
        string alterSql,
        CancellationToken cancellationToken) {
        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = $"PRAGMA table_info({tableName});";
        await using var reader = await checkCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) {
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
        }

        await using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = alterSql;
        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task OpenConnectionIfNeededAsync(DbConnection connection, CancellationToken cancellationToken) {
        if (connection.State != System.Data.ConnectionState.Open) {
            await connection.OpenAsync(cancellationToken);
        }
    }
}
