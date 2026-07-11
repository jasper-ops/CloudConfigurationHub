namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 项目级 Access Key 轮换结果。
/// </summary>
/// <param name="ProjectId">项目 ID。</param>
/// <param name="ProjectKey">项目 Key，用于 SDK 配置。</param>
/// <param name="AccessKey">只展示一次的 Access Key 明文。</param>
public sealed record ProjectAccessKeyRotationResult(Guid ProjectId, string ProjectKey, string AccessKey);
