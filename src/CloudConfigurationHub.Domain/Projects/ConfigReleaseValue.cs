namespace CloudConfigurationHub.Domain.Projects;

public sealed class ConfigReleaseValue
{
    internal ConfigReleaseValue(Guid configurationId, string configurationKey, string value, bool isSensitive)
    {
        ConfigurationId = configurationId;
        ConfigurationKey = configurationKey;
        Value = value;
        IsSensitive = isSensitive;
    }

    public Guid ConfigurationId { get; }

    public string ConfigurationKey { get; }

    public string Value { get; }

    public bool IsSensitive { get; }
}
