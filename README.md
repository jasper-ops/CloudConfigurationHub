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

发布到 NuGet.org 默认使用 Trusted Publishing；如果 NuGet.org 账号暂未配置 Trusted Publishing，可在仓库 Secrets 中配置 `NUGET_API_KEY` 作为兼容方案。
