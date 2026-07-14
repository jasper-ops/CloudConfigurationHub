namespace CloudConfigurationHub.App.Components;

internal sealed class ProjectFormModel {
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EnvironmentNames { get; set; } = string.Empty;
}

internal sealed class EnvironmentFormModel {
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}

internal sealed class ConfigFormModel {
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSensitive { get; set; }
    public Dictionary<Guid, string> Values { get; set; } = [];
}

internal sealed class PublishFormModel {
    public string Note { get; set; } = string.Empty;
    public string PublishedBy { get; set; } = "admin";
}

internal sealed class DiffPreviewRow {
    public string ConfigurationKey { get; set; } = string.Empty;
    public bool HasDraftValue { get; set; }
    public string DraftValue { get; set; } = string.Empty;
    public bool HasLatestReleaseValue { get; set; }
    public string LatestReleaseValue { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
}