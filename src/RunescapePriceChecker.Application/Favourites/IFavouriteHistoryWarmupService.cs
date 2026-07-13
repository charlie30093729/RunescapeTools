namespace RunescapePriceChecker.Application.Favourites;

public interface IFavouriteHistoryWarmupService
{
    Task WarmAsync(CancellationToken cancellationToken = default);
}
