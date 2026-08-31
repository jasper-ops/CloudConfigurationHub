using System.Reflection;
using CloudConfigurationHub.App.Components;

namespace CloudConfigurationHub.Tests.App;

public sealed class EnvironmentColorTests {
    [Fact]
    public void Default_environments_use_stable_distinct_colors() {
        var dotStyle = typeof(ManagementWorkbench).GetMethod(
            "DotStyle",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(dotStyle);

        var styles = new[] { "dev", "test", "prod" }
            .Select(key => Assert.IsType<string>(dotStyle.Invoke(null, [key])))
            .ToArray();

        Assert.Equal(styles.Length, styles.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal("background-color:#3b82f6", styles[0]);
        Assert.Equal("background-color:#f59e0b", styles[1]);
        Assert.Equal("background-color:#ef4444", styles[2]);
    }

    [Fact]
    public void Custom_environment_color_is_stable() {
        var dotStyle = typeof(ManagementWorkbench).GetMethod(
            "DotStyle",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(dotStyle);

        var first = Assert.IsType<string>(dotStyle.Invoke(null, ["preview"]));
        var second = Assert.IsType<string>(dotStyle.Invoke(null, ["preview"]));

        Assert.Equal(first, second);
    }
}
