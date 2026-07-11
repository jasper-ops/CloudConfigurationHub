using CloudConfigurationHub.App.Data;
using CloudConfigurationHub.App.Setup;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CloudConfigurationHub.Tests.App;

public sealed class SetupAdministratorServiceTests {
    [Fact]
    public async Task Setup_state_reports_incomplete_until_first_administrator_exists() {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();
        var setupState = scope.ServiceProvider.GetRequiredService<ISetupStateReader>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var before = await setupState.IsSetupCompletedAsync(CancellationToken.None);
        var user = new ApplicationUser {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, "Admin123!");
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(error => error.Description)));

        var after = await setupState.IsSetupCompletedAsync(CancellationToken.None);

        Assert.False(before);
        Assert.True(after);
    }

    [Fact]
    public async Task Setup_administrator_service_creates_exactly_one_confirmed_administrator() {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();
        var setupService = scope.ServiceProvider.GetRequiredService<ISetupAdministratorService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var created = await setupService.CreateInitialAdministratorAsync(
            new SetupAdministratorRequest("admin@example.com", "Admin123!"),
            CancellationToken.None);
        var duplicate = await setupService.CreateInitialAdministratorAsync(
            new SetupAdministratorRequest("other@example.com", "Admin123!"),
            CancellationToken.None);
        var users = userManager.Users.ToList();

        Assert.True(created.Succeeded, created.ErrorMessage);
        Assert.False(duplicate.Succeeded);
        Assert.Single(users);
        Assert.Equal("admin@example.com", users[0].Email);
        Assert.True(users[0].EmailConfirmed);
    }

    private static ServiceProvider BuildProvider() {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=:memory:"));
        services
            .AddIdentityCore<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<ISetupStateReader, IdentitySetupStateReader>();
        services.AddScoped<ISetupAdministratorService, IdentitySetupAdministratorService>();

        return services.BuildServiceProvider(new ServiceProviderOptions {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
