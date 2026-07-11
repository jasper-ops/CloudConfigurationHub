namespace CloudConfigurationHub.Sdk;

/// <summary>
/// SDK 从 SSE 流中接收的配置版本变更事件。
/// </summary>
/// <param name="ProjectId">发生变更的项目 ID 或项目 Key。</param>
/// <param name="EnvironmentKey">发生变更的环境 Key。</param>
/// <param name="Version">变更后的最新发布版本号。</param>
public sealed record ConfigurationChangedEvent(string ProjectId, string EnvironmentKey, int Version);
