namespace CloudConfigurationHub.Tests.App;

public sealed class ReleaseWorkflowTests {
    [Fact]
    public void Sdk_release_workflow_packs_and_publishes_packages_only_from_version_tags() {
        var workflowPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            ".github",
            "workflows",
            "release-sdk.yml");
        var sdkProjectPath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.Sdk",
            "CloudConfigurationHub.Sdk.csproj");

        var workflow = File.ReadAllText(Path.GetFullPath(workflowPath));
        var sdkProject = File.ReadAllText(Path.GetFullPath(sdkProjectPath));

        Assert.Contains("v*.*.*", workflow, StringComparison.Ordinal);
        Assert.Contains("v*.*.*-*", workflow, StringComparison.Ordinal);
        Assert.Contains("id-token: write", workflow, StringComparison.Ordinal);
        Assert.Contains("NuGet/login@v1", workflow, StringComparison.Ordinal);
        Assert.Contains("vars.NUGET_USER", workflow, StringComparison.Ordinal);
        Assert.Contains("steps.nuget-login.outputs.NUGET_API_KEY", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("secrets.NUGET_API_KEY", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet pack src/CloudConfigurationHub.Sdk/CloudConfigurationHub.Sdk.csproj", workflow, StringComparison.Ordinal);
        Assert.Contains("*.nupkg", workflow, StringComparison.Ordinal);
        Assert.Contains("*.snupkg", workflow, StringComparison.Ordinal);
        Assert.Contains("<PackageReadmeFile>README.md</PackageReadmeFile>", sdkProject, StringComparison.Ordinal);
        Assert.Contains("<PackageLicenseExpression>MIT</PackageLicenseExpression>", sdkProject, StringComparison.Ordinal);
        Assert.Contains("<SymbolPackageFormat>snupkg</SymbolPackageFormat>", sdkProject, StringComparison.Ordinal);
    }

    [Fact]
    public void Readme_documents_sdk_usage_and_nuget_release_prerequisites() {
        var readmePath = Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "README.md");

        var readme = File.ReadAllText(Path.GetFullPath(readmePath));

        Assert.Contains("AddCloudConfigurationHub", readme, StringComparison.Ordinal);
        Assert.Contains("X-CCH-Access-Key", readme, StringComparison.Ordinal);
        Assert.Contains("本地 JSON 缓存", readme, StringComparison.Ordinal);
        Assert.Contains("Trusted Publishing policy", readme, StringComparison.Ordinal);
        Assert.Contains("NUGET_USER", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("NUGET_API_KEY", readme, StringComparison.Ordinal);
    }
}
