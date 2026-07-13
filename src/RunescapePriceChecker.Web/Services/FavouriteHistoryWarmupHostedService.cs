using RunescapePriceChecker.Application.Favourites;

namespace RunescapePriceChecker.Web.Services;

public sealed class FavouriteHistoryWarmupHostedService(
    IFavouriteHistoryWarmupService warmup,
    ILogger<FavouriteHistoryWarmupHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await warmup.WarmAsync(stoppingToken);
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
