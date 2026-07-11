namespace CloudConfigurationHub.App.Endpoints;

/// <summary>
/// SDK 配置读取接口响应。
/// </summary>
/// <param name="Version">环境内最新发布版本号。</param>
/// <param name="Values">配置键值集合，键格式为 <c>Group:Key</c>。</param>
public sealed record SdkConfigurationResponse(int Version, IReadOnlyDictionary<string, string> Values);
