using RunescapePriceChecker.Core.Favourites;

namespace RunescapePriceChecker.Web.Services;

public sealed class FavouriteHistoryWarmupService(
    IFavouriteStore favouriteStore,
    MarketDataService marketData,
    ILogger<FavouriteHistoryWarmupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var favourites = await favouriteStore.GetAllAsync(stoppingToken);
            if (favourites.Count == 0)
                return;

            await marketData.GetLatestForAsync(favourites.Select(item => item.ItemId), stoppingToken);
            await Parallel.ForEachAsync(
                favourites,
                new ParallelOptions { CancellationToken = stoppingToken, MaxDegreeOfParallelism = 3 },
                async (favourite, cancellationToken) =>
                    await marketData.GetWeeklyHistoryAsync(favourite.ItemId, cancellationToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Favourite price history could not be warmed during startup.");
        }
    }
}
