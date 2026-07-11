namespace CloudConfigurationHub.Domain.Projects;

public sealed class ProjectEnvironment
{
    internal ProjectEnvironment(Guid id, string name, string key)
    {
        Id = id;
        Name = name;
        Key = key;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Key { get; }
}
