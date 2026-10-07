namespace RunescapeTools.Core.Favourites;

public sealed record FavouriteItem(int ItemId, string Name, DateTimeOffset AddedAt);

public interface IFavouriteStore
{
    Task<IReadOnlyList<FavouriteItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(FavouriteItem favourite, CancellationToken cancellationToken = default);

    Task RemoveAsync(int itemId, CancellationToken cancellationToken = default);

    /// <summary>Moves an existing favourite before another item, or to the end when the anchor is null.</summary>
    Task MoveBeforeAsync(int itemId, int? beforeItemId, CancellationToken cancellationToken = default);
}
