using CloudConfigurationHub.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

DemoOptions demoOptions;
try {
    demoOptions = DemoOptions.Parse(args);
    if (demoOptions.ShowHelp) {
        DemoOptions.PrintUsage();
        return;
    }

    demoOptions.Validate();
}
catch (InvalidOperationException exception) {
    Console.Error.WriteLine($"参数错误：{exception.Message}");
    Console.Error.WriteLine();
    DemoOptions.PrintUsage();
    Environment.ExitCode = 2;
    return;
}

Console.WriteLine("CloudConfigurationHub SDK Demo");
Console.WriteLine($"服务端：{demoOptions.Endpoint}");
Console.WriteLine($"项目：  {demoOptions.ProjectId}");
Console.WriteLine($"环境：  {demoOptions.EnvironmentKey}");
Console.WriteLine($"SSE：   {(demoOptions.EnableSse ? "已启用" : "已禁用")}");
Console.WriteLine($"缓存：  {Path.GetFullPath(demoOptions.CachePath)}");
Console.WriteLine();

IConfigurationRoot configuration;
try {
    configuration = new ConfigurationBuilder()
        .AddCloudConfigurationHub(options => {
            options.Endpoint = demoOptions.Endpoint!;
            options.ProjectId = demoOptions.ProjectId;
            options.EnvironmentKey = demoOptions.EnvironmentKey;
            options.AccessKey = demoOptions.AccessKey;
            options.LocalCachePath = demoOptions.CachePath;
            options.EnableSse = demoOptions.EnableSse;
            options.SseReconnectInterval = TimeSpan.FromSeconds(2);
        })
        .Build();
}
catch (Exception exception) {
    Console.Error.WriteLine("配置加载失败。请确认服务端已启动、项目和环境已发布，并且 Access Key 正确。");
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
    return;
}

using var configurationDisposable = configuration as IDisposable;
PrintConfiguration(configuration, "初始配置", demoOptions.ShowSensitiveValues);

if (!demoOptions.EnableSse) {
    Console.WriteLine("已完成首次配置加载测试。");
    return;
}

using var reloadRegistration = ChangeToken.OnChange(
    configuration.GetReloadToken,
    () => PrintConfiguration(configuration, "检测到配置更新", demoOptions.ShowSensitiveValues));

Console.WriteLine("Demo 正在监听配置变化。请在配置中心修改并发布当前环境，观察这里自动刷新。按 Ctrl+C 退出。");

var exitSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) => {
    eventArgs.Cancel = true;
    exitSource.TrySetResult();
};

Console.CancelKeyPress += cancelHandler;
try {
    await exitSource.Task;
}
finally {
    Console.CancelKeyPress -= cancelHandler;
}

static void PrintConfiguration(
    IConfiguration configuration,
    string title,
    bool showSensitiveValues) {
    var values = configuration
        .AsEnumerable()
        .Where(item => item.Value is not null)
        .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] {title}，共 {values.Length} 项：");
    if (values.Length == 0) {
        Console.WriteLine("  <当前发布版本没有配置项>");
    }
    else {
        foreach (var item in values) {
            var value = showSensitiveValues || !LooksSensitive(item.Key)
                ? item.Value
                : "******";
            Console.WriteLine($"  {item.Key} = {value}");
        }
    }

    Console.WriteLine();
}

static bool LooksSensitive(string key) {
    string[] sensitiveFragments = [
        "password",
        "passwd",
        "secret",
        "token",
        "accesskey",
        "apikey",
        "connectionstring"
    ];

    var normalizedKey = key.Replace(":", string.Empty, StringComparison.Ordinal);
    return sensitiveFragments.Any(fragment => normalizedKey.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}

internal sealed record DemoOptions(
    Uri? Endpoint,
    string ProjectId,
    string EnvironmentKey,
    string AccessKey,
    string CachePath,
    bool EnableSse,
    bool ShowSensitiveValues,
    bool ShowHelp) {
    public static DemoOptions Parse(string[] args) {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var enableSse = true;
        var showSensitiveValues = false;
        var showHelp = false;

        for (var index = 0; index < args.Length; index++) {
            var argument = args[index];
            switch (argument) {
                case "--no-sse":
                    enableSse = false;
                    break;
                case "--show-sensitive-values":
                    showSensitiveValues = true;
                    break;
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--endpoint":
                case "--project":
                case "--environment":
                case "--access-key":
                case "--cache":
                    if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal)) {
                        throw new InvalidOperationException($"参数“{argument}”缺少值。");
                    }

                    values[argument] = args[++index];
                    break;
                default:
                    throw new InvalidOperationException($"无法识别参数“{argument}”。");
            }
        }

        var endpointText = GetValue(values, "--endpoint", "CCH_ENDPOINT", "http://localhost:15137");
        var endpoint = Uri.TryCreate(endpointText, UriKind.Absolute, out var parsedEndpoint)
            ? parsedEndpoint
            : null;

        return new DemoOptions(
            endpoint,
            GetValue(values, "--project", "CCH_PROJECT", string.Empty),
            GetValue(values, "--environment", "CCH_ENVIRONMENT", "dev"),
            GetValue(values, "--access-key", "CCH_ACCESS_KEY", string.Empty),
            GetValue(values, "--cache", "CCH_CACHE_PATH", Path.Combine(".cache", "cloudconfigurationhub.json")),
            enableSse,
            showSensitiveValues,
            showHelp);
    }

    public void Validate() {
        if (Endpoint is null || (Endpoint.Scheme != Uri.UriSchemeHttp && Endpoint.Scheme != Uri.UriSchemeHttps)) {
            throw new InvalidOperationException("Endpoint 必须是有效的 HTTP 或 HTTPS 地址。");
        }

        if (string.IsNullOrWhiteSpace(ProjectId)) {
            throw new InvalidOperationException("请通过 --project 或 CCH_PROJECT 指定项目 Key。");
        }

        if (string.IsNullOrWhiteSpace(EnvironmentKey)) {
            throw new InvalidOperationException("请通过 --environment 或 CCH_ENVIRONMENT 指定环境 Key。");
        }

        if (string.IsNullOrWhiteSpace(AccessKey)) {
            throw new InvalidOperationException("请通过 --access-key 或 CCH_ACCESS_KEY 指定 Access Key。");
        }
    }

    public static void PrintUsage() {
        Console.WriteLine("用法：");
        Console.WriteLine("  dotnet run --project samples/CloudConfigurationHub.Sdk.Demo -- [选项]");
        Console.WriteLine();
        Console.WriteLine("选项：");
        Console.WriteLine("  --endpoint <url>              配置中心地址，默认 http://localhost:15137");
        Console.WriteLine("  --project <key>               项目 Key");
        Console.WriteLine("  --environment <key>           环境 Key，默认 dev");
        Console.WriteLine("  --access-key <key>            项目 Access Key");
        Console.WriteLine("  --cache <path>                本地缓存路径");
        Console.WriteLine("  --no-sse                      只测试首次加载，不监听实时更新");
        Console.WriteLine("  --show-sensitive-values       输出可能包含密码、密钥的配置值");
        Console.WriteLine("  -h, --help                    显示帮助");
        Console.WriteLine();
        Console.WriteLine("也可使用环境变量 CCH_ENDPOINT、CCH_PROJECT、CCH_ENVIRONMENT、CCH_ACCESS_KEY、CCH_CACHE_PATH。");
    }

    private static string GetValue(
        IReadOnlyDictionary<string, string> arguments,
        string argumentName,
        string environmentVariable,
        string defaultValue) {
        if (arguments.TryGetValue(argumentName, out var argumentValue)) {
            return argumentValue;
        }

        return Environment.GetEnvironmentVariable(environmentVariable) ?? defaultValue;
    }
}
