namespace CloudConfigurationHub.Application.Sdk;

/// <summary>
/// 面向 SDK 输出的已发布配置快照。
/// </summary>
/// <param name="Version">环境内发布版本号。</param>
/// <param name="Values">配置键值集合，键格式为 <c>Group:Key</c>。</param>
public sealed record PublishedConfigurationSnapshot(int Version, IReadOnlyDictionary<string, string> Values);
