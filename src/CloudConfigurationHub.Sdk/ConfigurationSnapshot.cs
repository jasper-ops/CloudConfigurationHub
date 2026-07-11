namespace CloudConfigurationHub.Sdk;

/// <summary>
/// SDK 从服务端获取的已发布配置快照。
/// </summary>
/// <param name="Version">服务端发布版本号。</param>
/// <param name="Values">配置键值集合，键格式为 <c>Group:Key</c>。</param>
public sealed record ConfigurationSnapshot(int Version, Dictionary<string, string> Values);
