using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 创建配置项目的命令。
/// </summary>
/// <param name="Name">项目显示名称，用于管理端列表和审计日志展示。</param>
/// <param name="Key">项目唯一 Key，用于 SDK 读取配置时定位项目。</param>
/// <param name="Description">项目说明。</param>
public sealed record CreateProjectCommand(string Name, string Key, string Description = "") : ICommand<ProjectSummary>;
