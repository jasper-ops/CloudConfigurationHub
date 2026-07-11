namespace CloudConfigurationHub.App.Setup;

/// <summary>
/// 创建首次启动管理员账号的应用服务。
/// </summary>
public interface ISetupAdministratorService {
    /// <summary>
    /// 创建唯一的初始管理员账号。
    /// </summary>
    /// <param name="request">创建管理员所需的邮箱与密码。</param>
    /// <param name="cancellationToken">取消异步操作的令牌。</param>
    /// <returns>创建结果。系统已经初始化或 Identity 校验失败时返回失败结果。</returns>
    Task<SetupAdministratorResult> CreateInitialAdministratorAsync(
        SetupAdministratorRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// 初始管理员创建请求。
/// </summary>
/// <param name="Email">管理员登录邮箱。</param>
/// <param name="Password">管理员登录密码。</param>
public sealed record SetupAdministratorRequest(string Email, string Password);

/// <summary>
/// 初始管理员创建结果。
/// </summary>
/// <param name="Succeeded">指示创建是否成功。</param>
/// <param name="ErrorMessage">失败时向管理端展示的错误消息。</param>
public sealed record SetupAdministratorResult(bool Succeeded, string? ErrorMessage) {
    /// <summary>
    /// 成功创建初始管理员账号的结果。
    /// </summary>
    public static SetupAdministratorResult Success { get; } = new(true, null);

    /// <summary>
    /// 创建失败的结果。
    /// </summary>
    /// <param name="errorMessage">失败原因。</param>
    /// <returns>失败结果。</returns>
    public static SetupAdministratorResult Failure(string errorMessage) => new(false, errorMessage);
}
