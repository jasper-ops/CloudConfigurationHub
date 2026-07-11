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
            "ReleaseHistory.PublishedAt"
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

        var pageSource = File.ReadAllText(Path.GetFullPath(pagePath));

        Assert.Contains("@onsubmit:preventDefault=\"true\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("type=\"button\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("@onclick=\"CreateProjectAsync\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", pageSource, StringComparison.Ordinal);
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

        var environmentSource = File.ReadAllText(Path.GetFullPath(environmentPagePath));
        var configurationSource = File.ReadAllText(Path.GetFullPath(configurationPagePath));

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/new\"", environmentSource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", environmentSource, StringComparison.Ordinal);
        Assert.Contains("new AddEnvironmentCommand", environmentSource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", environmentSource, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", environmentSource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", environmentSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/configurations/new\"", configurationSource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", configurationSource, StringComparison.Ordinal);
        Assert.Contains("new AddConfigurationCommand", configurationSource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", configurationSource, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", configurationSource, StringComparison.Ordinal);
        Assert.Contains("@bind=\"FormModel.IsSensitive\"", configurationSource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", configurationSource, StringComparison.Ordinal);
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

        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var environmentDraftSource = File.ReadAllText(Path.GetFullPath(environmentDraftPagePath));
        var configurationDraftSource = File.ReadAllText(Path.GetFullPath(configurationDraftPagePath));

        Assert.Contains("DraftEdit.EditEnvironment", projectDetailSource, StringComparison.Ordinal);
        Assert.Contains("DraftEdit.EditConfiguration", projectDetailSource, StringComparison.Ordinal);
        Assert.Contains("/drafts", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/{EnvironmentId:guid}/drafts\"", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("new GetProjectDetailQuery", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("new SetDraftValueCommand", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", environmentDraftSource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", environmentDraftSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/configurations/{ConfigurationId:guid}/drafts\"", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("new GetProjectDetailQuery", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("new SetDraftValueCommand", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", configurationDraftSource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", configurationDraftSource, StringComparison.Ordinal);
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

        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var publishSource = File.ReadAllText(Path.GetFullPath(publishPagePath));

        Assert.Contains("PublishEnvironment.PublishEnvironment", projectDetailSource, StringComparison.Ordinal);
        Assert.Contains("/publish", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/{EnvironmentId:guid}/publish\"", publishSource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", publishSource, StringComparison.Ordinal);
        Assert.Contains("new GetProjectDetailQuery", publishSource, StringComparison.Ordinal);
        Assert.Contains("new PublishEnvironmentCommand", publishSource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", publishSource, StringComparison.Ordinal);
        Assert.Contains("@bind:event=\"oninput\"", publishSource, StringComparison.Ordinal);
        Assert.Contains("ReleasePreviewRows", publishSource, StringComparison.Ordinal);
        Assert.Contains("DiffRows", publishSource, StringComparison.Ordinal);
        Assert.Contains("LatestReleaseValue", publishSource, StringComparison.Ordinal);
        Assert.Contains("DiffStatus", publishSource, StringComparison.Ordinal);
        Assert.Contains("item.ConfigurationKey == configurationKey", publishSource, StringComparison.Ordinal);
        Assert.Contains("PublishEnvironment.DiffTitle", publishSource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", publishSource, StringComparison.Ordinal);
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

        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var accessKeySource = File.ReadAllText(Path.GetFullPath(accessKeyPagePath));

        Assert.Contains("AccessKey.ManageAccessKey", projectDetailSource, StringComparison.Ordinal);
        Assert.Contains("/access-key", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/access-key\"", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("new GetProjectDetailQuery", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("new RotateProjectAccessKeyCommand", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("GeneratedAccessKey", accessKeySource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", accessKeySource, StringComparison.Ordinal);
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

        var projectDetailSource = File.ReadAllText(Path.GetFullPath(projectDetailPath));
        var releaseHistorySource = File.ReadAllText(Path.GetFullPath(releaseHistoryPagePath));

        Assert.Contains("ReleaseHistory.ManageHistory", projectDetailSource, StringComparison.Ordinal);
        Assert.Contains("/releases", projectDetailSource, StringComparison.Ordinal);

        Assert.Contains("@page \"/projects/{ProjectId:guid}/environments/{EnvironmentId:guid}/releases\"", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("@rendermode InteractiveServer", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("new GetProjectDetailQuery", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("new RollbackEnvironmentCommand", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("ReleaseRows", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("@onsubmit:preventDefault=\"true\"", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("@($\"v{release.Version}\")", releaseHistorySource, StringComparison.Ordinal);
        Assert.DoesNotContain("v@release.Version", releaseHistorySource, StringComparison.Ordinal);
        Assert.Contains("Logger.LogInformation", releaseHistorySource, StringComparison.Ordinal);
    }
}
