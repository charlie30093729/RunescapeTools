using RunescapePriceChecker.Core.Favourites;
using RunescapePriceChecker.Core.Market;
using RunescapePriceChecker.Core.MoneyMaking;
using RunescapePriceChecker.Core.MoneyMaking.Methods;
using RunescapePriceChecker.Web.Components;
using RunescapePriceChecker.Web.Infrastructure;
using RunescapePriceChecker.Web.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "data", "keys")));

builder.Services.AddHttpClient<IOsrsPriceClient, OsrsWikiPriceClient>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri("https://prices.runescape.wiki/api/v1/osrs/");
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        configuration["OsrsWiki:UserAgent"]
        ?? "RunescapePriceChecker/0.1 (contact: Discord bottleo)");
});

builder.Services.AddSingleton<IFavouriteStore, JsonFavouriteStore>();
builder.Services.AddSingleton<MarketDataService>();
builder.Services.AddSingleton<MoneyMakingCalculator>();
builder.Services.AddHostedService<FavouriteHistoryWarmupService>();

var moneyMakingMethodTypes = typeof(VyrewatchMethod).Assembly
    .GetTypes()
    .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IMoneyMakingMethod).IsAssignableFrom(type));

foreach (var methodType in moneyMakingMethodTypes)
    builder.Services.AddSingleton(typeof(IMoneyMakingMethod), methodType);

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
