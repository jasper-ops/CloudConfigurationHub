namespace CloudConfigurationHub.Domain.Projects;

public sealed class ConfigDefinition
{
    internal ConfigDefinition(Guid id, string group, string key, bool isSensitive)
    {
        Id = id;
        Group = group;
        Key = key;
        IsSensitive = isSensitive;
    }

    public Guid Id { get; }

    public string Group { get; }

    public string Key { get; }

    public bool IsSensitive { get; }
}
