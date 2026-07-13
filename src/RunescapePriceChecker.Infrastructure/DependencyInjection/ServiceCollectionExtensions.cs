using Microsoft.Extensions.DependencyInjection;
using RunescapePriceChecker.Application.Favourites;
using RunescapePriceChecker.Application.Market;
using RunescapePriceChecker.Core.Favourites;
using RunescapePriceChecker.Core.Market;
using RunescapePriceChecker.Core.MoneyMaking;
using RunescapePriceChecker.Infrastructure.Configuration;
using RunescapePriceChecker.Infrastructure.Market;
using RunescapePriceChecker.Infrastructure.Persistence;

namespace RunescapePriceChecker.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRuneScapePriceCheckerServices(
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
