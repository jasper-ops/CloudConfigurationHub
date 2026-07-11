namespace CloudConfigurationHub.Tests.App;

public sealed class RepositoryQualityTests {
    [Fact]
    public void Management_app_does_not_expose_template_demo_pages() {
        var pagesDirectory = Path.GetFullPath(Path.Combine(
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
        var forbiddenTemplatePages = new[] {
            "Counter.razor",
            "Weather.razor",
            "Auth.razor"
        };

        foreach (var page in forbiddenTemplatePages) {
            Assert.False(
                File.Exists(Path.Combine(pagesDirectory, page)),
                $"Template demo page should not be shipped: {page}");
        }
    }

    [Fact]
    public void Non_generated_csharp_files_keep_opening_braces_on_the_previous_line() {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));
        var checkedRoots = new[] {
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Domain"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Application"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Infrastructure"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Sdk"),
            Path.Combine(repositoryRoot, "tests")
        };
        var violations = checkedRoots
            .SelectMany(root => Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !Path.GetFileName(path).EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new {
                    Path = Path.GetRelativePath(repositoryRoot, path),
                    LineNumber = index + 1,
                    Line = line
                }))
            .Where(item => item.Line.Trim() == "{")
            .Select(item => $"{item.Path}:{item.LineNumber}")
            .ToArray();

        Assert.Empty(violations);
    }
}
