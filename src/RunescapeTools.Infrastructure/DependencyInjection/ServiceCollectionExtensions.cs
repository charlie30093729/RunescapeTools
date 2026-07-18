using Microsoft.Extensions.DependencyInjection;
using RunescapeTools.Application.Favourites;
using RunescapeTools.Application.Market;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Core.Favourites;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;
using RunescapeTools.Core.Profiles;
using RunescapeTools.Infrastructure.Configuration;
using RunescapeTools.Infrastructure.Market;
using RunescapeTools.Infrastructure.Persistence;
using RunescapeTools.Infrastructure.Profiles;

namespace RunescapeTools.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRunescapeToolsServices(
        this IServiceCollection services,
        OsrsWikiOptions wikiOptions,
        FavouriteStoreOptions favouriteOptions,
        MarketDataOptions? marketOptions = null,
        OsrsHiscoreOptions? hiscoreOptions = null)
    {
        hiscoreOptions ??= new OsrsHiscoreOptions { UserAgent = wikiOptions.UserAgent };
        services.AddSingleton(wikiOptions);
        services.AddSingleton(hiscoreOptions);
        services.AddSingleton(favouriteOptions);
        services.AddSingleton(marketOptions ?? new MarketDataOptions());
        services.AddSingleton(TimeProvider.System);

        services.AddHttpClient<IOsrsPriceClient, OsrsWikiPriceClient>(client =>
        {
            client.BaseAddress = wikiOptions.BaseAddress;
            client.Timeout = wikiOptions.Timeout;
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(wikiOptions.UserAgent);
        });

        services.AddHttpClient<IHiscoreClient, OsrsHiscoreClient>(client =>
        {
            client.BaseAddress = hiscoreOptions.BaseAddress;
            client.Timeout = hiscoreOptions.Timeout;
            client.DefaultRequestHeaders.Accept.ParseAdd("text/plain");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(hiscoreOptions.UserAgent);
        });

        services.AddSingleton<IFavouriteStore, JsonFavouriteStore>();
        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddSingleton<IFavouriteHistoryWarmupService, FavouriteHistoryWarmupService>();
        services.AddSingleton<HiscoreParser>();
        services.AddSingleton<MoneyMakingCalculator>();

        var methodTypes = typeof(IMoneyMakingMethod).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract
                           && !type.IsInterface
                           && typeof(IMoneyMakingMethod).IsAssignableFrom(type));

        foreach (var methodType in methodTypes)
            services.AddSingleton(typeof(IMoneyMakingMethod), methodType);

        return services;
    }
}
