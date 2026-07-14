namespace CloudConfigurationHub.Tests.App;

public sealed class SetupWizardSourceTests {
    [Fact]
    public void Setup_page_declares_first_run_route_form_and_observability() {
        var pagePath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Pages",
            "Setup.razor");

        var source = File.ReadAllText(Path.GetFullPath(pagePath));

        Assert.Contains("@page \"/setup\"", source, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", source, StringComparison.Ordinal);
        Assert.Contains("ISetupAdministratorService", source, StringComparison.Ordinal);
        Assert.Contains("ISetupStateReader", source, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", source, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", source, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", source, StringComparison.Ordinal);
        Assert.Contains("Setup.AdminCreated", source, StringComparison.Ordinal);
    }

    [Fact]
    public void App_shell_redirects_first_run_to_setup_and_requires_authorization_for_management_pages() {
        var programPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Program.cs");
        var importsPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Pages",
            "_Imports.razor");
        var registerPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Account",
            "Pages",
            "Register.razor");
        var setupExtensionsPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Setup",
            "StartupSetupExtensions.cs");
        var appPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "App.razor");

        var programSource = File.ReadAllText(Path.GetFullPath(programPath));
        var importsSource = File.ReadAllText(Path.GetFullPath(importsPath));
        var registerSource = File.ReadAllText(Path.GetFullPath(registerPath));
        var setupExtensionsSource = File.ReadAllText(Path.GetFullPath(setupExtensionsPath));
        var appSource = File.ReadAllText(Path.GetFullPath(appPath));

        Assert.Contains("MapStaticAssets().ShortCircuit()", programSource, StringComparison.Ordinal);
        Assert.Contains("UseStartupSetupRedirect", programSource, StringComparison.Ordinal);
        Assert.Contains("AddStartupSetup", programSource, StringComparison.Ordinal);
        Assert.Contains("InitializeIdentityDatabaseAsync", programSource, StringComparison.Ordinal);
        Assert.Contains("ApplicationDbContext", programSource, StringComparison.Ordinal);
        Assert.Contains("@attribute [Authorize]", importsSource, StringComparison.Ordinal);
        Assert.Contains("Setup.FirstRunOnly", registerSource, StringComparison.Ordinal);
        Assert.Contains("NavigationManager.NavigateTo(\"setup\"", registerSource, StringComparison.Ordinal);
        Assert.Contains("\"/_blazor\"", setupExtensionsSource, StringComparison.Ordinal);
        Assert.Contains("<HeadOutlet @rendermode=\"PageRenderMode\"", appSource, StringComparison.Ordinal);
        Assert.Contains("<Routes @rendermode=\"PageRenderMode\"", appSource, StringComparison.Ordinal);
        Assert.Contains("AcceptsInteractiveRouting()", appSource, StringComparison.Ordinal);
    }

    [Fact]
    public void App_pipeline_serves_static_framework_assets_before_setup_redirect() {
        var programPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Program.cs");

        var programSource = File.ReadAllText(Path.GetFullPath(programPath));
        var staticFilesIndex = programSource.IndexOf("app.UseStaticFiles();", StringComparison.Ordinal);
        var setupRedirectIndex = programSource.IndexOf("app.UseStartupSetupRedirect();", StringComparison.Ordinal);

        Assert.True(staticFilesIndex >= 0, "Program.cs must serve static framework assets.");
        Assert.Contains("app.MapStaticAssets().ShortCircuit();", programSource, StringComparison.Ordinal);
        Assert.True(
            staticFilesIndex < setupRedirectIndex,
            "Static files must be served before the first-run setup redirect middleware.");
    }

    [Fact]
    public void Management_pages_render_workbench_with_interactive_server_routes() {
        var pagesRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Pages"));
        var pagePaths = Directory
            .EnumerateFiles(pagesRoot, "*.razor", SearchOption.TopDirectoryOnly)
            .Where(path => File.ReadAllText(path).Contains("<ManagementWorkbench", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(pagePaths);
        foreach (var pagePath in pagePaths) {
            var source = File.ReadAllText(pagePath);

            Assert.Contains(
                "@rendermode InteractiveServer",
                source,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Workbench_child_actions_request_parent_render_for_sibling_dialogs() {
        var baseComponentPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Workbench",
            "ManagementWorkbenchComponentBase.cs"));
        var source = File.ReadAllText(baseComponentPath).ReplaceLineEndings("\n");

        Assert.Contains("set { Workbench.deleteConfigTarget = value; Workbench.RequestRender(); }", source, StringComparison.Ordinal);
        Assert.Contains("internal void OpenNewEnvironment() {\n        Workbench.OpenNewEnvironment();\n        Workbench.RequestRender();\n    }", source, StringComparison.Ordinal);
        Assert.Contains("internal void OpenReleaseHistoryPanel() {\n        Workbench.OpenReleaseHistoryPanel();\n        Workbench.RequestRender();\n    }", source, StringComparison.Ordinal);
        Assert.Contains("internal async Task SaveEnvironmentAsync() {\n        await Workbench.SaveEnvironmentAsync();\n        Workbench.RequestRender();\n    }", source, StringComparison.Ordinal);
    }
}
