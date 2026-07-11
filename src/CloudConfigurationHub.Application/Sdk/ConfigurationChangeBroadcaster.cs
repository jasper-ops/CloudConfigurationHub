using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace CloudConfigurationHub.Application.Sdk;

/// <summary>
/// 基于内存通道的配置变更广播器。
/// </summary>
public sealed class ConfigurationChangeBroadcaster : IConfigurationChangeBroadcaster {
    private readonly ConcurrentDictionary<SubscriptionKey, ConcurrentDictionary<Guid, Channel<ConfigurationChangedEvent>>> _subscriptions = [];

    /// <summary>
    /// 订阅某个项目环境的配置变更事件。
    /// </summary>
    /// <param name="projectId">项目 ID 或项目 Key。</param>
    /// <param name="environmentKey">环境 Key。</param>
    /// <param name="cancellationToken">取消令牌，用于终止订阅。</param>
    /// <returns>配置变更事件异步流。</returns>
    public async IAsyncEnumerable<ConfigurationChangedEvent> Subscribe(
        string projectId,
        string environmentKey,
        [EnumeratorCancellation] CancellationToken cancellationToken) {
        var key = SubscriptionKey.Create(projectId, environmentKey);
        var channel = Channel.CreateUnbounded<ConfigurationChangedEvent>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false
        });
        var subscriptionId = Guid.NewGuid();
        var subscribers = _subscriptions.GetOrAdd(key, _ => []);
        subscribers[subscriptionId] = channel;

        try {
            await foreach (var changedEvent in channel.Reader.ReadAllAsync(cancellationToken)) {
                yield return changedEvent;
            }
        }
        finally {
            subscribers.TryRemove(subscriptionId, out _);
            if (subscribers.IsEmpty) {
                _subscriptions.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// 向匹配项目环境的订阅者发布配置变更事件。
    /// </summary>
    /// <param name="changedEvent">配置变更事件。</param>
    /// <param name="cancellationToken">取消令牌，用于终止发布。</param>
    /// <returns>表示异步发布操作的值任务。</returns>
    public async ValueTask PublishAsync(ConfigurationChangedEvent changedEvent, CancellationToken cancellationToken) {
        var key = SubscriptionKey.Create(changedEvent.ProjectId, changedEvent.EnvironmentKey);
        if (!_subscriptions.TryGetValue(key, out var subscribers)) {
            return;
        }

        foreach (var channel in subscribers.Values) {
            await channel.Writer.WriteAsync(changedEvent, cancellationToken);
        }
    }

    private readonly record struct SubscriptionKey(string ProjectId, string EnvironmentKey) {
        public static SubscriptionKey Create(string projectId, string environmentKey) {
            return new SubscriptionKey(
                projectId.Trim().ToLowerInvariant(),
                environmentKey.Trim().ToLowerInvariant());
        }
    }
}
