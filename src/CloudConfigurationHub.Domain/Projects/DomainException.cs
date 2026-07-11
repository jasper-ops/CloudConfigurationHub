namespace CloudConfigurationHub.Domain.Projects;

/// <summary>
/// 表示违反领域规则的异常。
/// </summary>
/// <param name="message">面向调用方的领域错误消息。</param>
public sealed class DomainException(string message) : Exception(message);
