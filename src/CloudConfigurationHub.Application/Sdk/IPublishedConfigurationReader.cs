namespace CloudConfigurationHub.Application.Sdk;

/// <summary>
/// 面向 SDK 的已发布配置快照读取端口。
/// </summary>
public interface IPublishedConfigurationReader {
    /// <summary>
    /// 读取指定项目环境的最新已发布配置快照。
    /// </summary>
    /// <param name="projectId">项目 ID 或项目 Key。</param>
    /// <param name="environmentKey">环境 Key。</param>
    /// <param name="accessKey">项目级只读 Access Key。</param>
    /// <param name="cancellationToken">取消令牌，用于终止读取操作。</param>
    /// <returns>最新已发布配置快照；如果项目、环境或 Access Key 不匹配则返回 <see langword="null"/>。</returns>
    ValueTask<PublishedConfigurationSnapshot?> GetLatestAsync(
        string projectId,
        string environmentKey,
        string accessKey,
        CancellationToken cancellationToken);
}
