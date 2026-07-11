namespace CloudConfigurationHub.Application.Sdk;

/// <summary>
/// 配置变更事件广播端口。
/// </summary>
public interface IConfigurationChangeBroadcaster {
    /// <summary>
    /// 订阅某个项目环境的配置变更事件。
    /// </summary>
    /// <param name="projectId">项目 ID 或项目 Key。</param>
    /// <param name="environmentKey">环境 Key。</param>
    /// <param name="cancellationToken">取消令牌，用于终止订阅。</param>
    /// <returns>配置变更事件异步流。</returns>
    IAsyncEnumerable<ConfigurationChangedEvent> Subscribe(
        string projectId,
        string environmentKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// 发布配置变更事件。
    /// </summary>
    /// <param name="changedEvent">配置变更事件。</param>
    /// <param name="cancellationToken">取消令牌，用于终止发布。</param>
    /// <returns>表示异步发布操作的值任务。</returns>
    ValueTask PublishAsync(ConfigurationChangedEvent changedEvent, CancellationToken cancellationToken);
}
