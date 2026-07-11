namespace CloudConfigurationHub.Domain.Projects;

public sealed class Project
{
    private readonly List<ProjectEnvironment> _environments = [];
    private readonly List<ConfigDefinition> _configurations = [];
    private readonly List<ConfigDraftValue> _draftValues = [];
    private readonly List<ConfigRelease> _releases = [];

    private Project(Guid id, string name, string key)
    {
        Id = id;
        Name = name;
        Key = key;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Key { get; }

    public IReadOnlyCollection<ProjectEnvironment> Environments => _environments.AsReadOnly();

    public IReadOnlyCollection<ConfigDefinition> Configurations => _configurations.AsReadOnly();

    public IReadOnlyCollection<ConfigRelease> Releases => _releases.AsReadOnly();

    public static Project Create(string name, string key)
    {
        return new Project(Guid.NewGuid(), name, NormalizeKey(key));
    }

    public ProjectEnvironment AddEnvironment(string name, string key)
    {
        var normalizedKey = NormalizeKey(key);
        if (_environments.Any(environment => environment.Key == normalizedKey))
        {
            throw new DomainException("项目内环境 Key 必须唯一。");
        }

        var environment = new ProjectEnvironment(Guid.NewGuid(), name, normalizedKey);
        _environments.Add(environment);
        return environment;
    }

    public ConfigDefinition AddConfiguration(string group, string key, bool isSensitive)
    {
        var normalizedGroup = NormalizeKey(group);
        var normalizedKey = NormalizeKey(key);
        if (_configurations.Any(configuration =>
                configuration.Group == normalizedGroup && configuration.Key == normalizedKey))
        {
            throw new DomainException("项目内配置分组和 Key 组合必须唯一。");
        }

        var configuration = new ConfigDefinition(Guid.NewGuid(), normalizedGroup, normalizedKey, isSensitive);
        _configurations.Add(configuration);
        return configuration;
    }

    public void SetDraftValue(Guid environmentId, Guid configurationId, string value)
    {
        EnsureEnvironmentExists(environmentId);
        EnsureConfigurationExists(configurationId);

        var existingValue = _draftValues.SingleOrDefault(draftValue =>
            draftValue.EnvironmentId == environmentId && draftValue.ConfigurationId == configurationId);
        if (existingValue is null)
        {
            _draftValues.Add(new ConfigDraftValue(environmentId, configurationId, value));
            return;
        }

        existingValue.ReplaceValue(value);
    }

    public ConfigRelease PublishEnvironment(
        Guid environmentId,
        string note,
        string publishedBy,
        DateTimeOffset publishedAt)
    {
        EnsureEnvironmentExists(environmentId);
        var version = _releases.Count(release => release.EnvironmentId == environmentId) + 1;
        var values = _draftValues
            .Where(draftValue => draftValue.EnvironmentId == environmentId)
            .Select(draftValue =>
            {
                var configuration = _configurations.Single(item => item.Id == draftValue.ConfigurationId);
                return new ConfigReleaseValue(
                    configuration.Id,
                    $"{configuration.Group}:{configuration.Key}",
                    draftValue.Value,
                    configuration.IsSensitive);
            })
            .ToArray();

        var release = new ConfigRelease(
            Guid.NewGuid(),
            environmentId,
            version,
            note,
            publishedBy,
            publishedAt,
            values);
        _releases.Add(release);
        return release;
    }

    public ConfigRelease RollbackEnvironment(
        Guid environmentId,
        Guid sourceReleaseId,
        string note,
        string publishedBy,
        DateTimeOffset publishedAt)
    {
        EnsureEnvironmentExists(environmentId);
        var sourceRelease = _releases.SingleOrDefault(release =>
            release.EnvironmentId == environmentId && release.Id == sourceReleaseId);
        if (sourceRelease is null)
        {
            throw new DomainException("回滚源版本不存在。");
        }

        var version = _releases.Count(release => release.EnvironmentId == environmentId) + 1;
        var values = sourceRelease.Values
            .Select(value => new ConfigReleaseValue(
                value.ConfigurationId,
                value.ConfigurationKey,
                value.Value,
                value.IsSensitive))
            .ToArray();

        var release = new ConfigRelease(
            Guid.NewGuid(),
            environmentId,
            version,
            note,
            publishedBy,
            publishedAt,
            values);
        _releases.Add(release);
        return release;
    }

    private void EnsureEnvironmentExists(Guid environmentId)
    {
        if (_environments.All(environment => environment.Id != environmentId))
        {
            throw new DomainException("环境不存在。");
        }
    }

    private void EnsureConfigurationExists(Guid configurationId)
    {
        if (_configurations.All(configuration => configuration.Id != configurationId))
        {
            throw new DomainException("配置项不存在。");
        }
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToLowerInvariant();
    }
}
