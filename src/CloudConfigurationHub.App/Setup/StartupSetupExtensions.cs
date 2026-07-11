namespace CloudConfigurationHub.App.Setup;

/// <summary>
/// 启动向导相关的依赖注入与请求管道扩展。
/// </summary>
public static class StartupSetupExtensions {
    /// <summary>
    /// 注册启动向导服务。
    /// </summary>
    /// <param name="services">应用服务集合。</param>
    /// <returns>原服务集合，便于链式配置。</returns>
    public static IServiceCollection AddStartupSetup(this IServiceCollection services) {
        services.AddScoped<ISetupStateReader, IdentitySetupStateReader>();
        services.AddScoped<ISetupAdministratorService, IdentitySetupAdministratorService>();
        return services;
    }

    /// <summary>
    /// 在首次启动且尚未创建管理员时，将页面访问重定向到启动向导。
    /// </summary>
    /// <param name="app">应用请求管道。</param>
    /// <returns>原应用构建器，便于链式配置。</returns>
    public static IApplicationBuilder UseStartupSetupRedirect(this IApplicationBuilder app) =>
        app.Use(async (context, next) => {
            if (ShouldSkipSetupRedirect(context.Request)) {
                await next();
                return;
            }

            var setupStateReader = context.RequestServices.GetRequiredService<ISetupStateReader>();
            if (await setupStateReader.IsSetupCompletedAsync(context.RequestAborted)) {
                await next();
                return;
            }

            context.Response.Redirect("/setup");
        });

    /// <summary>
    /// 判断当前请求是否应绕过启动向导重定向。
    /// </summary>
    /// <param name="request">当前 HTTP 请求。</param>
    /// <returns>静态资源、SDK API 和 setup 页面返回 <c>true</c>。</returns>
    private static bool ShouldSkipSetupRedirect(HttpRequest request) {
        if (!HttpMethods.IsGet(request.Method)) {
            return true;
        }

        var path = request.Path;
        return path.StartsWithSegments("/setup")
            || path.StartsWithSegments("/api")
            || path.StartsWithSegments("/_blazor")
            || path.StartsWithSegments("/_framework")
            || path.StartsWithSegments("/_content")
            || path.StartsWithSegments("/lib")
            || path.StartsWithSegments("/Components")
            || path.Value?.EndsWith(".css", StringComparison.OrdinalIgnoreCase) == true
            || path.Value?.EndsWith(".js", StringComparison.OrdinalIgnoreCase) == true
            || path.Value?.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == true
            || path.Value?.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) == true;
    }
}
