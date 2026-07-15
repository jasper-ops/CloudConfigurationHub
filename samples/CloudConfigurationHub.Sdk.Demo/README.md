# CloudConfigurationHub SDK Demo

这个控制台程序用于验证 SDK 的四项核心能力：

- 启动时从配置中心读取最新发布快照；
- 将 `Group:Key` 配置映射为标准 `IConfiguration` 键值；
- 收到 SSE 发布事件后自动刷新配置；
- 服务端暂时不可用时使用本地 JSON 缓存启动。

## 1. 启动配置中心

```powershell
dotnet run --project src/CloudConfigurationHub.App --launch-profile http
```

浏览器打开 `http://localhost:15137`，完成初始化登录后：

1. 创建一个项目，例如项目 Key 为 `sdk-demo`；
2. 创建环境，例如环境 Key 为 `dev`；
3. 添加几个便于观察的配置，例如 `Demo:Message`、`Feature:Enabled`、`Limits:MaxItems`；
4. 填写环境值并发布；
5. 在项目的“访问密钥”中轮换密钥，立即复制只展示一次的 Access Key。

## 2. 运行 Demo

推荐通过环境变量传递 Access Key，避免密钥出现在 shell 历史中：

```powershell
$env:CCH_ENDPOINT = "http://localhost:15137"
$env:CCH_PROJECT = "sdk-demo"
$env:CCH_ENVIRONMENT = "dev"
$env:CCH_ACCESS_KEY = "替换为刚生成的 Access Key"

dotnet run --project samples/CloudConfigurationHub.Sdk.Demo
```

程序会输出当前发布的全部配置。保持程序运行，在管理后台修改环境值并再次发布，控制台应在收到 SSE 事件后打印“检测到配置更新”和新值。

## 3. 测试本地缓存降级

Demo 成功连接一次后，会在当前目录生成 `.cache/cloudconfigurationhub.json`。停止配置中心，再使用相同命令启动 Demo，SDK 会从缓存加载最近一次成功读取的快照。

本地缓存是明文 JSON，仅应用于测试。生产环境使用时应限制文件权限，并确保缓存目录不会提交到版本库。

查看全部参数：

```powershell
dotnet run --project samples/CloudConfigurationHub.Sdk.Demo -- --help
```

如果只想验证首次加载而不保持 SSE 长连接，可添加 `--no-sse`；程序输出快照后会直接退出。
