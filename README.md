# CloudConfigurationHub

CloudConfigurationHub 是一个面向 .NET 应用的云配置中心 MVP，目标是提供项目/环境/分组/配置项管理、发布版本、SSE 配置变更通知，以及 `.NET Configuration Provider` SDK。

## 技术栈

- ASP.NET Core 10
- Blazor Web App Interactive Server
- Fluent UI Blazor
- DDD + CQRS + TDD
- Mediator
- EF Core + SQLite
- .NET SDK package: `CloudConfigurationHub.Sdk`

## 开发命令

```powershell
dotnet restore
dotnet build -warnaserror
dotnet test
```

## SDK 发布

推送符合 SemVer 的 tag 会触发 SDK 发布 workflow：

```powershell
git tag v1.0.0
git push origin v1.0.0
```

发布到 NuGet.org 默认使用 Trusted Publishing；需要先在 NuGet.org 为本仓库创建 Trusted Publishing policy，并在 GitHub 仓库变量中配置 `NUGET_USER`。如果 NuGet.org 账号暂未配置 Trusted Publishing，可在仓库 Secrets 中配置 `NUGET_API_KEY` 作为兼容方案。

本地发布前验证：

```powershell
dotnet pack src/CloudConfigurationHub.Sdk/CloudConfigurationHub.Sdk.csproj `
  --configuration Release `
  --output artifacts/packages-local `
  /p:Version=0.1.0-local.1 `
  /p:ContinuousIntegrationBuild=true
```

该命令应生成 `.nupkg` 和 `.snupkg` 两类包。

## SDK 接入示例

服务端发布配置后，.NET 应用可以通过 `CloudConfigurationHub.Sdk` 接入 `IConfiguration`：

```csharp
using CloudConfigurationHub.Sdk;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddCloudConfigurationHub(options => {
    options.BaseAddress = new Uri("https://config.example.com");
    options.ProjectId = Guid.Parse("00000000-0000-0000-0000-000000000000");
    options.EnvironmentKey = "prod";
    options.AccessKey = builder.Configuration["CloudConfigurationHub:AccessKey"]!;
    options.LocalCachePath = "configuration-cache/cloudconfigurationhub.json";
    options.EnableStreamingRefresh = true;
});
```

SDK 请求服务端时会发送 `X-CCH-Access-Key`。配置输出 Key 使用 `Group:Key` 格式，例如 `Database:ConnectionString`，可直接交给 Options 绑定。

SDK 默认保留内存缓存，并在远端可用时刷新本地 JSON 缓存。远端不可用但本地 JSON 缓存存在时会降级启动；远端不可用且没有本地缓存时启动失败。本地 JSON 缓存为明文文件，生产环境应使用操作系统文件权限限制读取范围，不要提交到仓库。
