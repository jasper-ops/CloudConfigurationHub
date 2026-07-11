using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 为项目添加配置定义的命令。
/// </summary>
/// <param name="ProjectId">项目聚合 ID。</param>
/// <param name="Group">配置分组。</param>
/// <param name="Key">配置 Key。</param>
/// <param name="IsSensitive">是否为敏感配置。</param>
public sealed record AddConfigurationCommand(Guid ProjectId, string Group, string Key, bool IsSensitive)
    : ICommand<ConfigurationDefinitionSummary>;
