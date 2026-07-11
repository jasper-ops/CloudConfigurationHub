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
            "Projects.ProjectCount"
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
}
