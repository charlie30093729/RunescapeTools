using Microsoft.Extensions.DependencyInjection;
using RunescapeTools.Application.Favourites;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Favourites;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;
using RunescapeTools.Infrastructure.Configuration;
using RunescapeTools.Infrastructure.Market;
using RunescapeTools.Infrastructure.Persistence;

namespace RunescapeTools.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRunescapeToolsServices(
        this IServiceCollection services,
        OsrsWikiOptions wikiOptions,
        FavouriteStoreOptions favouriteOptions,
        MarketDataOptions? marketOptions = null)
    {
        services.AddSingleton(wikiOptions);
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

        services.AddSingleton<IFavouriteStore, JsonFavouriteStore>();
        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddSingleton<IFavouriteHistoryWarmupService, FavouriteHistoryWarmupService>();
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
