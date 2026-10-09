using RunescapeTools.Application.Market;
using RunescapeTools.Core.Favourites;

namespace RunescapeTools.Application.Favourites;

public sealed class FavouriteHistoryWarmupService(
    IFavouriteStore favouriteStore,
    IMarketDataService marketData,
    MarketDataOptions options) : IFavouriteHistoryWarmupService
{
    public async Task WarmAsync(CancellationToken cancellationToken = default)
    {
        var favourites = await favouriteStore.GetAllAsync(cancellationToken);
        if (favourites.Count == 0)
            return;

        await marketData.GetLatestForAsync(favourites.Select(item => item.ItemId), cancellationToken);
        await Parallel.ForEachAsync(
            favourites,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Math.Max(1, options.WarmupConcurrency)
            },
            async (favourite, token) =>
                await marketData.GetWeeklyHistoryAsync(favourite.ItemId, token));
    }
}
