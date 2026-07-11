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

        var programSource = File.ReadAllText(Path.GetFullPath(programPath));
        var importsSource = File.ReadAllText(Path.GetFullPath(importsPath));
        var registerSource = File.ReadAllText(Path.GetFullPath(registerPath));
        var setupExtensionsSource = File.ReadAllText(Path.GetFullPath(setupExtensionsPath));

        Assert.Contains("MapStaticAssets().ShortCircuit()", programSource, StringComparison.Ordinal);
        Assert.Contains("UseStartupSetupRedirect", programSource, StringComparison.Ordinal);
        Assert.Contains("AddStartupSetup", programSource, StringComparison.Ordinal);
        Assert.Contains("InitializeIdentityDatabaseAsync", programSource, StringComparison.Ordinal);
        Assert.Contains("ApplicationDbContext", programSource, StringComparison.Ordinal);
        Assert.Contains("@attribute [Authorize]", importsSource, StringComparison.Ordinal);
        Assert.Contains("Setup.FirstRunOnly", registerSource, StringComparison.Ordinal);
        Assert.Contains("NavigationManager.NavigateTo(\"setup\"", registerSource, StringComparison.Ordinal);
        Assert.Contains("\"/_blazor\"", setupExtensionsSource, StringComparison.Ordinal);
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
}
