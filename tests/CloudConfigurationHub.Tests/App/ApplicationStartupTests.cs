using CloudConfigurationHub.Application;
using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Infrastructure;
using CloudConfigurationHub.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CloudConfigurationHub.Tests.App;

public sealed class ApplicationStartupTests {
    [Fact]
    public void BuildServiceProvider_validates_mediator_handlers_with_scoped_infrastructure_services() {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(options => {
            options.Assemblies = [typeof(CreateProjectCommand).Assembly];
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
        services.AddDbContext<ConfigurationHubDbContext>(options =>
            options.UseSqlite("Data Source=:memory:"));
        services.AddScoped<IProjectRepository, EfProjectRepository>();
        services.AddScoped<IProjectReadModel, EfProjectReadModel>();
        services.AddScoped<IPublishedConfigurationReader, EfPublishedConfigurationReader>();
        services.AddSingleton<IAccessKeyHasher, Sha256AccessKeyHasher>();
        services.AddSingleton<IConfigurationChangeBroadcaster, ConfigurationChangeBroadcaster>();
        services.AddSingleton<IClock, SystemClock>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        Assert.NotNull(sender);
    }
}
