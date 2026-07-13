namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 配置中心中的项目聚合根。
/// </summary>
/// <remarks>
/// 项目聚合负责维护环境、配置定义、草稿值和发布版本之间的一致性规则。
/// </remarks>
public sealed class Project {
    private readonly List<ProjectEnvironment> _environments = [];
    private readonly List<ConfigDefinition> _configurations = [];
    private readonly List<ConfigDraftValue> _draftValues = [];
    private readonly List<ConfigRelease> _releases = [];

    private Project() {
        Name = string.Empty;
        Key = string.Empty;
        Description = string.Empty;
        AccessKeyHash = string.Empty;
    }

    private Project(Guid id, string name, string key, string description, DateTimeOffset createdAt) {
        Id = id;
        Name = name;
        Key = key;
        Description = description;
        CreatedAt = createdAt;
        AccessKeyHash = string.Empty;
    }

    /// <summary>
    /// 项目 ID。
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// 项目显示名称。
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 项目唯一 Key，用于 SDK 读取配置时定位项目。
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// 项目说明，用于管理端展示业务用途。
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 项目创建时间。
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// 项目级只读 Access Key 的哈希值。
    /// </summary>
    public string AccessKeyHash { get; private set; }

    /// <summary>
    /// 项目下已定义的环境集合。
    /// </summary>
    public IReadOnlyCollection<ProjectEnvironment> Environments => _environments.AsReadOnly();

    /// <summary>
    /// 项目下统一维护的配置定义集合。
    /// </summary>
    public IReadOnlyCollection<ConfigDefinition> Configurations => _configurations.AsReadOnly();

    /// <summary>
    /// 项目下所有环境的发布版本集合。
    /// </summary>
    public IReadOnlyCollection<ConfigRelease> Releases => _releases.AsReadOnly();

    /// <summary>
    /// 项目下所有环境的配置草稿值集合。
    /// </summary>
    public IReadOnlyCollection<ConfigDraftValue> DraftValues => _draftValues.AsReadOnly();

    /// <summary>
    /// 创建一个新的配置项目。
    /// </summary>
    /// <param name="name">项目显示名称。</param>
    /// <param name="key">项目唯一 Key，会被标准化为小写。</param>
    /// <returns>新建的项目聚合。</returns>
    public static Project Create(string name, string key) {
        return Create(name, key, string.Empty, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 创建一个新的配置项目。
    /// </summary>
    /// <param name="name">项目显示名称。</param>
    /// <param name="key">项目唯一 Key，会被标准化为小写。</param>
    /// <param name="description">项目说明。</param>
    /// <param name="createdAt">项目创建时间。</param>
    /// <returns>新建的项目聚合。</returns>
    public static Project Create(string name, string key, string description, DateTimeOffset createdAt) {
        return new Project(Guid.NewGuid(), name, NormalizeKey(key), description.Trim(), createdAt);
    }

    /// <summary>
    /// 更新项目基础信息。
    /// </summary>
    /// <param name="name">项目显示名称。</param>
    /// <param name="key">项目唯一 Key，会被标准化为小写。</param>
    /// <param name="description">项目说明。</param>
    public void UpdateDetails(string name, string key, string description) {
        Name = name.Trim();
        Key = NormalizeKey(key);
        Description = description.Trim();
    }

    /// <summary>
    /// 为项目添加一个环境。
    /// </summary>
    /// <param name="name">环境显示名称。</param>
    /// <param name="key">环境唯一 Key，会被标准化为小写。</param>
    /// <returns>新建的项目环境。</returns>
    /// <exception cref="DomainException">当环境 Key 在项目内重复时抛出。</exception>
    public ProjectEnvironment AddEnvironment(string name, string key) {
        var normalizedKey = NormalizeKey(key);
        if (_environments.Any(environment => environment.Key == normalizedKey)) {
            throw new DomainException("项目内环境 Key 必须唯一。");
        }

        var environment = new ProjectEnvironment(Guid.NewGuid(), name, normalizedKey);
        _environments.Add(environment);
        return environment;
    }

    /// <summary>
    /// 更新项目环境基础信息。
    /// </summary>
    /// <param name="environmentId">环境 ID。</param>
    /// <param name="name">环境显示名称。</param>
    /// <param name="key">环境唯一 Key，会被标准化为小写。</param>
    /// <returns>更新后的项目环境。</returns>
    public ProjectEnvironment UpdateEnvironment(Guid environmentId, string name, string key) {
        var environment = _environments.SingleOrDefault(item => item.Id == environmentId)
            ?? throw new DomainException("环境不存在。");
        var normalizedKey = NormalizeKey(key);
        if (_environments.Any(item => item.Id != environmentId && item.Key == normalizedKey)) {
            throw new DomainException("项目内环境 Key 必须唯一。");
        }

        environment.Update(name, normalizedKey);
        return environment;
    }

    /// <summary>
    /// 添加项目级配置定义。
    /// </summary>
    /// <param name="group">配置分组，会被标准化为小写。</param>
    /// <param name="key">配置 Key，会被标准化为小写。</param>
    /// <param name="isSensitive">是否为敏感配置。</param>
    /// <returns>新建的配置定义。</returns>
    /// <exception cref="DomainException">当分组和 Key 组合在项目内重复时抛出。</exception>
    public ConfigDefinition AddConfiguration(string group, string key, bool isSensitive, string description = "") {
        var normalizedGroup = NormalizeKey(group);
        var normalizedKey = NormalizeKey(key);
        if (_configurations.Any(configuration =>
                configuration.Group == normalizedGroup && configuration.Key == normalizedKey)) {
            throw new DomainException("项目内配置分组和 Key 组合必须唯一。");
        }

        var configuration = new ConfigDefinition(Guid.NewGuid(), normalizedGroup, normalizedKey, isSensitive, description.Trim());
        _configurations.Add(configuration);
        return configuration;
    }

    /// <summary>
    /// 更新项目级配置定义。
    /// </summary>
    /// <param name="configurationId">配置定义 ID。</param>
    /// <param name="group">配置分组，会被标准化为小写。</param>
    /// <param name="key">配置 Key，会被标准化为小写。</param>
    /// <param name="isSensitive">是否为敏感配置。</param>
    /// <param name="description">配置说明。</param>
    /// <returns>更新后的配置定义。</returns>
    public ConfigDefinition UpdateConfiguration(
        Guid configurationId,
        string group,
        string key,
        bool isSensitive,
        string description) {
        var configuration = _configurations.SingleOrDefault(item => item.Id == configurationId)
            ?? throw new DomainException("配置项不存在。");
        var normalizedGroup = NormalizeKey(group);
        var normalizedKey = NormalizeKey(key);
        if (_configurations.Any(item =>
                item.Id != configurationId && item.Group == normalizedGroup && item.Key == normalizedKey)) {
            throw new DomainException("项目内配置分组和 Key 组合必须唯一。");
        }

        configuration.Update(normalizedGroup, normalizedKey, isSensitive, description.Trim());
        return configuration;
    }

    /// <summary>
    /// 删除项目级配置定义及其草稿值。
    /// </summary>
    /// <param name="configurationId">配置定义 ID。</param>
    public void RemoveConfiguration(Guid configurationId) {
        var configuration = _configurations.SingleOrDefault(item => item.Id == configurationId)
            ?? throw new DomainException("配置项不存在。");
        _configurations.Remove(configuration);
        _draftValues.RemoveAll(item => item.ConfigurationId == configurationId);
    }

    /// <summary>
    /// 替换项目级只读 Access Key 的哈希值。
    /// </summary>
    /// <param name="accessKeyHash">新的 Access Key 哈希值。</param>
    public void ReplaceAccessKeyHash(string accessKeyHash) {
        AccessKeyHash = accessKeyHash;
    }

    /// <summary>
    /// 设置某个环境下某个配置定义的草稿值。
    /// </summary>
    /// <param name="environmentId">目标环境 ID。</param>
    /// <param name="configurationId">目标配置定义 ID。</param>
    /// <param name="value">草稿字符串值。</param>
    public void SetDraftValue(Guid environmentId, Guid configurationId, string value) {
        EnsureEnvironmentExists(environmentId);
        EnsureConfigurationExists(configurationId);

        var existingValue = _draftValues.SingleOrDefault(draftValue =>
            draftValue.EnvironmentId == environmentId && draftValue.ConfigurationId == configurationId);
        if (existingValue is null) {
            _draftValues.Add(new ConfigDraftValue(environmentId, configurationId, value));
            return;
        }

        existingValue.ReplaceValue(value);
    }

    /// <summary>
    /// 将某个环境当前草稿发布为新的不可变版本。
    /// </summary>
    /// <param name="environmentId">发布目标环境 ID。</param>
    /// <param name="note">发布备注。</param>
    /// <param name="publishedBy">发布人标识。</param>
    /// <param name="publishedAt">发布时间。</param>
    /// <returns>新生成的发布版本。</returns>
    public ConfigRelease PublishEnvironment(
        Guid environmentId,
        string note,
        string publishedBy,
        DateTimeOffset publishedAt) {
        EnsureEnvironmentExists(environmentId);
        var version = _releases.Count(release => release.EnvironmentId == environmentId) + 1;
        var values = _draftValues
            .Where(draftValue => draftValue.EnvironmentId == environmentId)
            .Select(draftValue => {
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

    /// <summary>
    /// 从指定历史版本生成新的发布版本，实现一键回滚。
    /// </summary>
    /// <param name="environmentId">回滚目标环境 ID。</param>
    /// <param name="sourceReleaseId">作为回滚源的历史发布版本 ID。</param>
    /// <param name="note">回滚发布备注。</param>
    /// <param name="publishedBy">执行回滚的管理员标识。</param>
    /// <param name="publishedAt">回滚发布时间。</param>
    /// <returns>回滚生成的新发布版本。</returns>
    /// <exception cref="DomainException">当回滚源版本不存在时抛出。</exception>
    public ConfigRelease RollbackEnvironment(
        Guid environmentId,
        Guid sourceReleaseId,
        string note,
        string publishedBy,
        DateTimeOffset publishedAt) {
        EnsureEnvironmentExists(environmentId);
        var sourceRelease = _releases.SingleOrDefault(release =>
            release.EnvironmentId == environmentId && release.Id == sourceReleaseId);
        if (sourceRelease is null) {
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

    private void EnsureEnvironmentExists(Guid environmentId) {
        if (_environments.All(environment => environment.Id != environmentId)) {
            throw new DomainException("环境不存在。");
        }
    }

    private void EnsureConfigurationExists(Guid configurationId) {
        if (_configurations.All(configuration => configuration.Id != configurationId)) {
            throw new DomainException("配置项不存在。");
        }
    }

    private static string NormalizeKey(string key) {
        return key.Trim().ToLowerInvariant();
    }
}
