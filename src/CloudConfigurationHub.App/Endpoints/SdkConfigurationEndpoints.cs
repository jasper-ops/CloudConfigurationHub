using CloudConfigurationHub.Application.Sdk;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CloudConfigurationHub.App.Endpoints;

/// <summary>
/// 面向 .NET SDK 的配置读取 HTTP endpoint。
/// </summary>
public static class SdkConfigurationEndpoints {
    /// <summary>
    /// 注册 SDK 配置读取 endpoint。
    /// </summary>
    /// <param name="app">应用 endpoint 路由构建器。</param>
    /// <returns>传入的 endpoint 路由构建器，便于链式调用。</returns>
    public static IEndpointRouteBuilder MapSdkConfigurationEndpoints(this IEndpointRouteBuilder app) {
        var group = app.MapGroup("/api/sdk/v1/projects/{projectId}/environments/{environmentKey}")
            .WithTags("SDK Configuration");

        group.MapGet("/configuration", GetConfigurationAsync)
            .WithName("GetSdkConfiguration")
            .WithSummary("Get the latest published configuration snapshot for a project environment.")
            .Produces<SdkConfigurationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    /// <summary>
    /// 获取指定项目环境的最新已发布配置快照。
    /// </summary>
    /// <param name="projectId">项目 ID 或项目 Key。</param>
    /// <param name="environmentKey">环境 Key。</param>
    /// <param name="accessKey">请求头 <c>X-CCH-Access-Key</c> 中的项目级只读密钥。</param>
    /// <param name="sender">CQRS 查询分发器。</param>
    /// <param name="cancellationToken">取消令牌，用于终止查询。</param>
    /// <returns>配置快照响应；认证失败或未发布时返回 401。</returns>
    public static async ValueTask<Results<Ok<SdkConfigurationResponse>, UnauthorizedHttpResult>> GetConfigurationAsync(
        string projectId,
        string environmentKey,
        [Microsoft.AspNetCore.Mvc.FromHeader(Name = "X-CCH-Access-Key")] string accessKey,
        ISender sender,
        CancellationToken cancellationToken) {
        var snapshot = await sender.Send(
            new GetPublishedConfigurationQuery(projectId, environmentKey, accessKey),
            cancellationToken);
        if (snapshot is null) {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(new SdkConfigurationResponse(snapshot.Version, snapshot.Values));
    }
}
