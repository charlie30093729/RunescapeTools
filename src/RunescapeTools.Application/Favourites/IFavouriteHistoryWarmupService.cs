namespace RunescapeTools.Application.Favourites;

public interface IFavouriteHistoryWarmupService
{
    Task WarmAsync(CancellationToken cancellationToken = default);
}
