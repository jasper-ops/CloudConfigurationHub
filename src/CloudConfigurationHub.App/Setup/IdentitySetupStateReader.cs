using CloudConfigurationHub.App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CloudConfigurationHub.App.Setup;

/// <summary>
/// 基于 ASP.NET Core Identity 用户表读取启动向导状态。
/// </summary>
public sealed class IdentitySetupStateReader(UserManager<ApplicationUser> userManager) : ISetupStateReader {
    /// <summary>
    /// Identity 用户管理器，用于读取当前是否存在用户。
    /// </summary>
    private readonly UserManager<ApplicationUser> userManager = userManager;

    /// <inheritdoc />
    public Task<bool> IsSetupCompletedAsync(CancellationToken cancellationToken) =>
        userManager.Users.AnyAsync(cancellationToken);
}
