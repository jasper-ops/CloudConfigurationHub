using CloudConfigurationHub.App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CloudConfigurationHub.App.Setup;

/// <summary>
/// 基于 ASP.NET Core Identity 创建唯一初始管理员账号。
/// </summary>
public sealed class IdentitySetupAdministratorService(
    UserManager<ApplicationUser> userManager,
    ILogger<IdentitySetupAdministratorService> logger) : ISetupAdministratorService {
    /// <summary>
    /// Identity 用户管理器，用于创建管理员账号。
    /// </summary>
    private readonly UserManager<ApplicationUser> userManager = userManager;

    /// <summary>
    /// 结构化日志记录器，用于审计启动向导行为。
    /// </summary>
    private readonly ILogger<IdentitySetupAdministratorService> logger = logger;

    /// <inheritdoc />
    public async Task<SetupAdministratorResult> CreateInitialAdministratorAsync(
        SetupAdministratorRequest request,
        CancellationToken cancellationToken) {
        var normalizedEmail = request.Email.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEmail)) {
            return SetupAdministratorResult.Failure("管理员邮箱不能为空。");
        }

        if (await userManager.Users.AnyAsync(cancellationToken)) {
            logger.LogWarning(
                "拒绝重复执行启动向导管理员创建。RequestedEmail={Email}",
                normalizedEmail);
            return SetupAdministratorResult.Failure("系统已经完成初始化，不能再次创建初始管理员。");
        }

        var user = new ApplicationUser {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) {
            var errorMessage = string.Join(" ", result.Errors.Select(error => error.Description));
            logger.LogWarning(
                "启动向导创建管理员失败。Email={Email}, IdentityErrors={IdentityErrors}",
                normalizedEmail,
                errorMessage);
            return SetupAdministratorResult.Failure(errorMessage);
        }

        logger.LogInformation(
            "启动向导已创建初始管理员账号。UserId={UserId}, Email={Email}",
            user.Id,
            normalizedEmail);
        return SetupAdministratorResult.Success;
    }
}
