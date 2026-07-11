namespace CloudConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// 配置中心数据库结构初始化器。
/// </summary>
public interface IConfigurationHubDatabaseInitializer {
    /// <summary>
    /// 确保配置中心所需数据库表已经存在。
    /// </summary>
    /// <param name="cancellationToken">取消令牌，用于终止数据库初始化。</param>
    /// <returns>表示数据库初始化过程的异步任务。</returns>
    Task InitializeAsync(CancellationToken cancellationToken);
}
