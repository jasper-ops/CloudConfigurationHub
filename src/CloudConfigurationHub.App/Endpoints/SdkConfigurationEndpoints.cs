using CloudConfigurationHub.Application.Sdk;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

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

        group.MapGet("/configuration/stream", StreamConfigurationChangesAsync)
            .WithName("StreamSdkConfigurationChanges")
            .WithSummary("Stream configuration version changes for a project environment using Server-Sent Events.")
            .Produces(StatusCodes.Status200OK, contentType: "text/event-stream");

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

    /// <summary>
    /// 使用 SSE 输出指定项目环境的配置版本变更事件。
    /// </summary>
    /// <param name="projectId">项目 ID 或项目 Key。</param>
    /// <param name="environmentKey">环境 Key。</param>
    /// <param name="broadcaster">配置变更广播器。</param>
    /// <param name="response">HTTP 响应对象。</param>
    /// <param name="cancellationToken">取消令牌，用于终止长连接。</param>
    /// <returns>表示 SSE 输出过程的异步任务。</returns>
    public static async ValueTask StreamConfigurationChangesAsync(
        string projectId,
        string environmentKey,
        IConfigurationChangeBroadcaster broadcaster,
        HttpResponse response,
        CancellationToken cancellationToken) {
        response.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        await response.WriteAsync(": connected\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);

        await foreach (var changedEvent in broadcaster.Subscribe(projectId, environmentKey, cancellationToken)) {
            var json = JsonSerializer.Serialize(changedEvent, JsonOptions);
            await response.WriteAsync("event: version-changed\n", cancellationToken);
            await response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
