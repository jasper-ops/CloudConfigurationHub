using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CloudConfigurationHub.App.Endpoints;
using CloudConfigurationHub.App.Components;
using CloudConfigurationHub.App.Components.Account;
using CloudConfigurationHub.App.Data;
using CloudConfigurationHub.Application;
using CloudConfigurationHub.Application.Projects;
using CloudConfigurationHub.Application.Sdk;
using CloudConfigurationHub.Infrastructure;
using CloudConfigurationHub.Infrastructure.Persistence;
using CloudConfigurationHub.Infrastructure.Security;
using CloudConfigurationHub.Application.Security;
using CloudConfigurationHub.App.Setup;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();
builder.Services.AddLocalization();
builder.Services.Configure<ConfigurationValueProtectionOptions>(
    builder.Configuration.GetSection("ConfigurationHub:Protection"));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorization();
builder.Services.AddMediator(options => {
    options.Assemblies = [typeof(CreateProjectCommand).Assembly];
    options.ServiceLifetime = ServiceLifetime.Scoped;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDbContext<ConfigurationHubDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddScoped<IProjectRepository, EfProjectRepository>();
builder.Services.AddScoped<IProjectReadModel, EfProjectReadModel>();
builder.Services.AddScoped<IPublishedConfigurationReader, EfPublishedConfigurationReader>();
builder.Services.AddScoped<IConfigurationHubDatabaseInitializer, ConfigurationHubDatabaseInitializer>();
builder.Services.AddSingleton<IAccessKeyHasher, Sha256AccessKeyHasher>();
builder.Services.AddSingleton<IAccessKeyGenerator, RandomAccessKeyGenerator>();
builder.Services.AddSingleton<ISecretProtector, AesGcmSecretProtector>();
builder.Services.AddSingleton<IConfigurationChangeBroadcaster, ConfigurationChangeBroadcaster>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddStartupSetup();

var app = builder.Build();
await InitializeIdentityDatabaseAsync(app);
await InitializeConfigurationHubDatabaseAsync(app);

var supportedCultures = new[] {
    new CultureInfo("zh-CN"),
    new CultureInfo("en-US"),
    new CultureInfo("ja-JP"),
    new CultureInfo("ko-KR"),
    new CultureInfo("es-ES")
};
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("zh-CN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
    app.MapOpenApi();
}
else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStartupSetupRedirect();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapSdkConfigurationEndpoints();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

static async Task InitializeConfigurationHubDatabaseAsync(WebApplication app) {
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IConfigurationHubDatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
}

static async Task InitializeIdentityDatabaseAsync(WebApplication app) {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await AlignLegacyIdentityMigrationHistoryAsync(dbContext, CancellationToken.None);
    await dbContext.Database.MigrateAsync(CancellationToken.None);
}

static async Task AlignLegacyIdentityMigrationHistoryAsync(
    ApplicationDbContext dbContext,
    CancellationToken cancellationToken) {
    const string legacyMigrationId = "00000000000000_CreateIdentitySchema";
    var currentMigrationId = dbContext.Database.GetMigrations()
        .FirstOrDefault(migrationId => migrationId.EndsWith("_CreateIdentitySchema", StringComparison.Ordinal));
    if (currentMigrationId is null || currentMigrationId == legacyMigrationId) {
        return;
    }

    var historyRepository = dbContext.Database.GetService<IHistoryRepository>();
    if (!await historyRepository.ExistsAsync(cancellationToken)) {
        return;
    }

    var appliedMigrations = await historyRepository.GetAppliedMigrationsAsync(cancellationToken);
    var legacyMigration = appliedMigrations.SingleOrDefault(
        migration => migration.MigrationId == legacyMigrationId);
    if (legacyMigration is null
        || appliedMigrations.Any(migration => migration.MigrationId == currentMigrationId)) {
        return;
    }

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    await dbContext.Database.ExecuteSqlRawAsync(
        historyRepository.GetDeleteScript(legacyMigrationId),
        cancellationToken);
    await dbContext.Database.ExecuteSqlRawAsync(
        historyRepository.GetInsertScript(
            new HistoryRow(currentMigrationId, ProductInfo.GetVersion())),
        cancellationToken);
    await transaction.CommitAsync(cancellationToken);
}

/// <summary>
/// Web 应用程序入口点类型，用于集成测试通过 <c>WebApplicationFactory</c> 构建宿主。
/// </summary>
public partial class Program;
