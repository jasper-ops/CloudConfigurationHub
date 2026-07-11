using Mediator;

namespace CloudConfigurationHub.Application.Sdk;

/// <summary>
/// SDK 读取某项目某环境最新已发布配置快照的查询。
/// </summary>
/// <param name="ProjectId">项目 ID 或项目 Key。</param>
/// <param name="EnvironmentKey">环境 Key。</param>
/// <param name="AccessKey">项目级只读 Access Key。</param>
public sealed record GetPublishedConfigurationQuery(string ProjectId, string EnvironmentKey, string AccessKey)
    : IQuery<PublishedConfigurationSnapshot?>;
