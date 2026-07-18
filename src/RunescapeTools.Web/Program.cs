using RunescapeTools.Web.Components;
using RunescapeTools.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using RunescapeTools.Application.Market;
using RunescapeTools.Infrastructure.Configuration;
using RunescapeTools.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "data", "keys")));

var dataDirectory = builder.Configuration["DataDirectory"] ?? "data";
builder.Services.AddRunescapeToolsServices(
    new OsrsWikiOptions
    {
        UserAgent = builder.Configuration["OsrsWiki:UserAgent"]
                    ?? "RunescapeTools/0.1 (contact: Discord bottleo)"
    },
    new FavouriteStoreOptions
    {
        FilePath = Path.Combine(builder.Environment.ContentRootPath, dataDirectory, "favourites.json")
    },
    new MarketDataOptions());
builder.Services.AddHostedService<FavouriteHistoryWarmupHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
