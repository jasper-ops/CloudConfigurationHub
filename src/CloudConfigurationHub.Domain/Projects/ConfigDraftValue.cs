namespace CloudConfigurationHub.Domain.Projects;

public sealed class ConfigDraftValue
{
    internal ConfigDraftValue(Guid environmentId, Guid configurationId, string value)
    {
        EnvironmentId = environmentId;
        ConfigurationId = configurationId;
        Value = value;
    }

    public Guid EnvironmentId { get; }

    public Guid ConfigurationId { get; }

    public string Value { get; private set; }

    internal void ReplaceValue(string value)
    {
        Value = value;
    }
}
