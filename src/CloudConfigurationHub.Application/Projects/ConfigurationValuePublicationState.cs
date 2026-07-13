namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 管理端配置草稿值相对最新发布版本的状态。
/// </summary>
public enum ConfigurationValuePublicationState {
    /// <summary>
    /// 草稿和最新发布版本均未设置该配置值。
    /// </summary>
    NotSet,

    /// <summary>
    /// 草稿已设置，但最新发布版本尚未包含该配置值。
    /// </summary>
    NotPublished,

    /// <summary>
    /// 草稿值与最新发布版本一致。
    /// </summary>
    Published,

    /// <summary>
    /// 草稿值已变更，尚未发布为最新版本。
    /// </summary>
    PendingPublish,

    /// <summary>
    /// 最新发布版本仍包含该配置值，但草稿中已不再设置。
    /// </summary>
    PendingRemoval
}
