using System.Globalization;
using CloudConfigurationHub.App.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace CloudConfigurationHub.Tests.App;

public sealed class AppLocalizationTests {
    [Fact]
    public void Localizer_returns_chinese_text_by_default_and_english_text_for_en_us() {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        using var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<IStringLocalizer<AppText>>();
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("zh-CN");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            var chineseTitle = localizer["Projects.Title"].Value;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var englishTitle = localizer["Projects.Title"].Value;

            Assert.Equal("项目配置中心", chineseTitle);
            Assert.Equal("Project configuration center", englishTitle);
        }
        finally {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void Localizer_contains_project_create_and_detail_page_keys_for_supported_cultures() {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        using var provider = services.BuildServiceProvider();
        var localizer = provider.GetRequiredService<IStringLocalizer<AppText>>();
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        var requiredKeys = new[] {
            "ProjectCreate.PageTitle",
            "ProjectCreate.Title",
            "ProjectCreate.NameLabel",
            "ProjectCreate.KeyLabel",
            "ProjectCreate.Submit",
            "ProjectCreate.GuideTitle",
            "ProjectCreate.GuideProject",
            "ProjectCreate.GuideEnvironment",
            "ProjectCreate.GuideRelease",
            "ProjectCreate.ValidationRequired",
            "ProjectDetail.PageTitle",
            "ProjectDetail.Environments",
            "ProjectDetail.Configurations",
            "ProjectDetail.NoEnvironments",
            "ProjectDetail.NoConfigurations",
            "ProjectDetail.BackToProjects",
            "ProjectDetail.Sensitive",
            "Projects.ProjectCount",
            "Theme.Switcher",
            "Theme.System",
            "Theme.Light",
            "Theme.Dark",
            "EnvironmentCreate.PageTitle",
            "EnvironmentCreate.Title",
            "EnvironmentCreate.NameLabel",
            "EnvironmentCreate.KeyLabel",
            "EnvironmentCreate.Submit",
            "EnvironmentCreate.ValidationRequired",
            "ConfigurationCreate.PageTitle",
            "ConfigurationCreate.Title",
            "ConfigurationCreate.GroupLabel",
            "ConfigurationCreate.KeyLabel",
            "ConfigurationCreate.IsSensitiveLabel",
            "ConfigurationCreate.Submit",
            "ConfigurationCreate.ValidationRequired",
            "DraftEdit.EditEnvironment",
            "DraftEdit.EditConfiguration",
            "EnvironmentDraftEdit.PageTitle",
            "EnvironmentDraftEdit.Title",
            "EnvironmentDraftEdit.ValueLabel",
            "EnvironmentDraftEdit.Submit",
            "EnvironmentDraftEdit.NoConfigurations",
            "ConfigurationDraftEdit.PageTitle",
            "ConfigurationDraftEdit.Title",
            "ConfigurationDraftEdit.ValueLabel",
            "ConfigurationDraftEdit.Submit",
            "ConfigurationDraftEdit.NoEnvironments",
            "PublishEnvironment.PageTitle",
            "PublishEnvironment.Title",
            "PublishEnvironment.Submit",
            "PublishEnvironment.NoteLabel",
            "PublishEnvironment.PublishedByLabel",
            "PublishEnvironment.NoConfigurations",
            "PublishEnvironment.PreviewTitle",
            "PublishEnvironment.PublishEnvironment",
            "PublishEnvironment.DiffTitle",
            "PublishEnvironment.DiffStatus",
            "PublishEnvironment.LatestPublishedValue",
            "PublishEnvironment.DiffAdded",
            "PublishEnvironment.DiffModified",
            "PublishEnvironment.DiffRemoved",
            "PublishEnvironment.DiffUnchanged",
            "PublishEnvironment.NoLatestRelease",
            "AccessKey.PageTitle",
            "AccessKey.Title",
            "AccessKey.Description",
            "AccessKey.Rotate",
            "AccessKey.GeneratedLabel",
            "AccessKey.GeneratedHelp",
            "AccessKey.Eyebrow",
            "AccessKey.Rotating",
            "AccessKey.Warning",
            "AccessKey.ManageAccessKey",
            "ReleaseHistory.PageTitle",
            "ReleaseHistory.Title",
            "ReleaseHistory.History",
            "ReleaseHistory.NoReleases",
            "ReleaseHistory.Rollback",
            "ReleaseHistory.RollingBack",
            "ReleaseHistory.ManageHistory",
            "ReleaseHistory.Version",
            "ReleaseHistory.Note",
            "ReleaseHistory.PublishedBy",
            "ReleaseHistory.PublishedAt",
            "TopNav.OperationalStatus",
            "TopNav.Connected",
            "TopNav.SdkBoundary",
            "Setup.PageTitle",
            "Setup.Title",
            "Setup.Description",
            "Setup.EmailLabel",
            "Setup.PasswordLabel",
            "Setup.ConfirmPasswordLabel",
            "Setup.Submit",
            "Setup.FirstRunOnly",
            "Setup.AdminCreated"
        };

        try {
            foreach (var cultureName in new[] { "zh-CN", "en-US" }) {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
                foreach (var key in requiredKeys) {
                    var value = localizer[key];

                    Assert.False(value.ResourceNotFound, $"{cultureName} missing resource key {key}.");
                    Assert.False(string.IsNullOrWhiteSpace(value.Value), $"{cultureName} resource key {key} is empty.");
                }
            }
        }
        finally {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void Project_create_page_uses_interactive_button_submission_without_native_post() {
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
            "ProjectCreate.razor");
        var workbenchPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ManagementWorkbench.razor");

        var pageSource = File.ReadAllText(Path.GetFullPath(pagePath));
        var workbenchSource = File.ReadAllText(Path.GetFullPath(workbenchPath));

        Assert.Contains("@page \"/projects/new\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("RoutePanel=\"project-new\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("type=\"button\"", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("SaveProjectAsync", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("@bind=\"projectForm.Name\"", workbenchSource, StringComparison.Ordinal);
    }

    [Fact]
    public void App_shell_declares_theme_script_and_three_mode_theme_switcher() {
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
        var layoutPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Layout",
            "MainLayout.razor");
        var switcherPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ThemeSwitcher.razor");
        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "wwwroot",
            "theme.js");
        var enhancedScriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "wwwroot",
            "theme-enhanced.js");

        var appSource = File.ReadAllText(Path.GetFullPath(appPath));
        var layoutSource = File.ReadAllText(Path.GetFullPath(layoutPath));
        var switcherSource = File.ReadAllText(Path.GetFullPath(switcherPath));
        var scriptSource = File.ReadAllText(Path.GetFullPath(scriptPath));
        var enhancedScriptSource = File.ReadAllText(Path.GetFullPath(enhancedScriptPath));

        Assert.Contains("theme.js", appSource, StringComparison.Ordinal);
        Assert.Contains("theme-enhanced.js", appSource, StringComparison.Ordinal);
        Assert.Contains("<ThemeSwitcher", layoutSource, StringComparison.Ordinal);
        Assert.Contains("data-theme-switcher", switcherSource, StringComparison.Ordinal);
        Assert.Contains("data-theme-option=\"system\"", switcherSource, StringComparison.Ordinal);
        Assert.Contains("data-theme-option=\"light\"", switcherSource, StringComparison.Ordinal);
        Assert.Contains("data-theme-option=\"dark\"", switcherSource, StringComparison.Ordinal);
        Assert.Contains("<svg", switcherSource, StringComparison.Ordinal);
        Assert.Contains("prefers-color-scheme: dark", scriptSource, StringComparison.Ordinal);
        Assert.Contains("localStorage", scriptSource, StringComparison.Ordinal);
        Assert.Contains("data-theme", scriptSource, StringComparison.Ordinal);
        Assert.Contains("enhancedload", scriptSource, StringComparison.Ordinal);
        Assert.Contains("pageshow", scriptSource, StringComparison.Ordinal);
        Assert.Contains("Blazor.addEventListener(\"enhancedload\"", enhancedScriptSource, StringComparison.Ordinal);
    }

    [Fact]
    public void App_shell_uses_non_fingerprinted_blazor_framework_script() {
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

        var appSource = File.ReadAllText(Path.GetFullPath(appPath));

        Assert.Contains("<script src=\"_framework/blazor.web.js\"></script>", appSource, StringComparison.Ordinal);
        Assert.DoesNotContain("@Assets[\"_framework/blazor.web.js\"]", appSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Language_switcher_disables_enhanced_navigation_for_request_culture_changes() {
        var switcherPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "LanguageSwitcher.razor");

        var switcherSource = File.ReadAllText(Path.GetFullPath(switcherPath));

        Assert.Contains("data-enhance-nav=\"false\"", switcherSource, StringComparison.Ordinal);
        Assert.Contains("ui-culture", switcherSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Forgot_password_pages_use_shared_login_page_visual_shell() {
        var forgotPasswordPath = RepositoryFile(
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Account",
            "Pages",
            "ForgotPassword.razor");
        var confirmationPath = RepositoryFile(
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Account",
            "Pages",
            "ForgotPasswordConfirmation.razor");

        var forgotPasswordSource = File.ReadAllText(forgotPasswordPath);
        var confirmationSource = File.ReadAllText(confirmationPath);

        foreach (var source in new[] { forgotPasswordSource, confirmationSource }) {
            Assert.Contains("cch-login-shell", source, StringComparison.Ordinal);
            Assert.Contains("cch-login-header", source, StringComparison.Ordinal);
            Assert.Contains("cch-login-card", source, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"row\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("form-floating", source, StringComparison.Ordinal);
            Assert.DoesNotContain("btn-lg", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Two_factor_login_pages_use_shared_login_page_visual_shell() {
        var twoFactorPath = RepositoryFile(
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Account",
            "Pages",
            "LoginWith2fa.razor");
        var recoveryCodePath = RepositoryFile(
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Account",
            "Pages",
            "LoginWithRecoveryCode.razor");

        var twoFactorSource = File.ReadAllText(twoFactorPath);
        var recoveryCodeSource = File.ReadAllText(recoveryCodePath);

        foreach (var source in new[] { twoFactorSource, recoveryCodeSource }) {
            Assert.Contains("@layout PlainLayout", source, StringComparison.Ordinal);
            Assert.Contains("cch-login-shell", source, StringComparison.Ordinal);
            Assert.Contains("cch-login-header", source, StringComparison.Ordinal);
            Assert.Contains("cch-login-card", source, StringComparison.Ordinal);
            Assert.Contains("cch-form-stack", source, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"row\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("form-floating", source, StringComparison.Ordinal);
            Assert.DoesNotContain("btn-lg", source, StringComparison.Ordinal);
        }
    }

    private static string RepositoryFile(params string[] pathParts) {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory }) {
            var directory = new DirectoryInfo(start);
            while (directory is not null) {
                var marker = Path.Combine(directory.FullName, "CloudConfigurationHub.slnx");
                if (File.Exists(marker)) {
                    return Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException($"Could not locate repository root for {Path.Combine(pathParts)}.");
    }

    [Fact]
    public void Project_detail_create_child_pages_declare_routes_and_interactive_commands() {
        var environmentPagePath = Path.Combine(
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
            "EnvironmentCreate.razor");
        var configurationPagePath = Path.Combine(
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
            "ConfigurationCreate.razor");
        var workbenchPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ManagementWorkbench.razor");

        var environmentSource = File.ReadAllText(Path.GetFullPath(environmentPagePath));
        var configurationSource = File.ReadAllText(Path.GetFullPath(configurationPagePath));
        var workbenchSource = File.ReadAllText(Path.GetFullPath(workbenchPath));

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/new\"", environmentSource, StringComparison.Ordinal);
        Assert.Contains("RoutePanel=\"environment-new\"", environmentSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/configurations/new\"", configurationSource, StringComparison.Ordinal);
        Assert.Contains("RoutePanel=\"configuration-new\"", configurationSource, StringComparison.Ordinal);
        Assert.Contains("new AddEnvironmentCommand", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("OpenEditEnvironment", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("new UpdateEnvironmentCommand", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("new AddConfigurationCommand", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("@bind=\"configForm.IsSensitive\"", workbenchSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Project_detail_draft_edit_pages_declare_routes_and_save_commands() {
        var projectDetailPath = Path.Combine(
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
            "ProjectDetail.razor");
        var environmentDraftPagePath = Path.Combine(
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
            "EnvironmentDraftEdit.razor");
        var configurationDraftPagePath = Path.Combine(
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
            "ConfigurationDraftEdit.razor");

        var workbenchPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ManagementWorkbench.razor");
        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var environmentDraftSource = File.ReadAllText(Path.GetFullPath(environmentDraftPagePath));
        var configurationDraftSource = File.ReadAllText(Path.GetFullPath(configurationDraftPagePath));
        var workbenchSource = File.ReadAllText(Path.GetFullPath(workbenchPath));

        Assert.Contains("ProjectId=\"ProjectId\"", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/{EnvironmentId:guid}/drafts\"", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("EnvironmentId=\"EnvironmentId\"", environmentDraftSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/configurations/{ConfigurationId:guid}/drafts\"", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("ConfigurationId=\"ConfigurationId\"", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("new GetProjectDetailQuery", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("new SetDraftValueCommand", workbenchSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Project_detail_publish_page_declares_route_preview_and_publish_command() {
        var projectDetailPath = Path.Combine(
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
            "ProjectDetail.razor");
        var publishPagePath = Path.Combine(
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
            "EnvironmentPublish.razor");

        var workbenchPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ManagementWorkbench.razor");
        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var publishSource = File.ReadAllText(Path.GetFullPath(publishPagePath));
        var workbenchSource = File.ReadAllText(Path.GetFullPath(workbenchPath));

        Assert.Contains("ProjectId=\"ProjectId\"", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/{EnvironmentId:guid}/publish\"", publishSource, StringComparison.Ordinal);
        Assert.Contains("RoutePanel=\"publish\"", publishSource, StringComparison.Ordinal);
        Assert.Contains("new PublishEnvironmentCommand", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("DiffRows", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("LatestReleaseValue", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("item.ConfigurationKey == configurationKey", workbenchSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Project_detail_access_key_page_declares_route_and_rotate_command() {
        var projectDetailPath = Path.Combine(
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
            "ProjectDetail.razor");
        var accessKeyPagePath = Path.Combine(
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
            "ProjectAccessKey.razor");

        var workbenchPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ManagementWorkbench.razor");
        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var accessKeySource = File.ReadAllText(Path.GetFullPath(accessKeyPagePath));
        var workbenchSource = File.ReadAllText(Path.GetFullPath(workbenchPath));

        Assert.Contains("ProjectId=\"ProjectId\"", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/access-key\"", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("RoutePanel=\"access-key\"", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("new RotateProjectAccessKeyCommand", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("generatedAccessKey", workbenchSource, StringComparison.Ordinal);
    }

    [Fact]
    public void Project_detail_release_history_page_declares_route_history_and_rollback_command() {
        var projectDetailPath = Path.Combine(
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
            "ProjectDetail.razor");
        var releaseHistoryPagePath = Path.Combine(
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
            "EnvironmentReleaseHistory.razor");

        var workbenchPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "ManagementWorkbench.razor");
        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var releaseHistorySource = File.ReadAllText(Path.GetFullPath(releaseHistoryPagePath));
        var workbenchSource = File.ReadAllText(Path.GetFullPath(workbenchPath));

        Assert.Contains("ProjectId=\"ProjectId\"", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/{EnvironmentId:guid}/releases\"", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("RoutePanel=\"releases\"", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("new RollbackEnvironmentCommand", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("ReleaseRows", workbenchSource, StringComparison.Ordinal);
        Assert.Contains("@($\"v{release.Version}\")", workbenchSource, StringComparison.Ordinal);
        Assert.DoesNotContain("v@release.Version", workbenchSource, StringComparison.Ordinal);
    }
}
