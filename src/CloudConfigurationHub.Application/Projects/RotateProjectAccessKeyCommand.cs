using Mediator;

namespace CloudConfigurationHub.Application.Projects;

/// <summary>
/// 轮换项目级只读 Access Key 的命令。
/// </summary>
/// <param name="ProjectId">目标项目 ID。</param>
/// <param name="RotatedBy">执行轮换的管理员标识。</param>
public sealed record RotateProjectAccessKeyCommand(Guid ProjectId, string RotatedBy)
    : ICommand<ProjectAccessKeyRotationResult>;
