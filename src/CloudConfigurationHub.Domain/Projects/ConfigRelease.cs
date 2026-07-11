namespace CloudConfigurationHub.Domain.Projects;

public sealed class ConfigRelease
{
    internal ConfigRelease(
        Guid id,
        Guid environmentId,
        int version,
        string note,
        string publishedBy,
        DateTimeOffset publishedAt,
        IReadOnlyCollection<ConfigReleaseValue> values)
    {
        Id = id;
        EnvironmentId = environmentId;
        Version = version;
        Note = note;
        PublishedBy = publishedBy;
        PublishedAt = publishedAt;
        Values = values;
    }

    public Guid Id { get; }

    public Guid EnvironmentId { get; }

    public int Version { get; }

    public string Note { get; }

    public string PublishedBy { get; }

    public DateTimeOffset PublishedAt { get; }

    public IReadOnlyCollection<ConfigReleaseValue> Values { get; }
}
