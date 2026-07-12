using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 更新配置定义和环境草稿值的命令。
/// </summary>
public sealed record UpdateConfigurationCommand(
    Guid ProjectId,
    Guid ConfigurationId,
    string Group,
    string Key,
    bool IsSensitive,
    string Description,
    IReadOnlyDictionary<Guid, string> EnvironmentValues) : ICommand<ConfigurationDefinitionSummary>;
