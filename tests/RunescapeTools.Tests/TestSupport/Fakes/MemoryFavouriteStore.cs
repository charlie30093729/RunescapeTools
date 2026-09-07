namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class MemoryFavouriteStore(params FavouriteItem[] initial) : IFavouriteStore
{
    private readonly List<FavouriteItem> items = [.. initial];

    public Task<IReadOnlyList<FavouriteItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FavouriteItem>>(items.OrderBy(item => item.Name).ToArray());

    public Task AddAsync(FavouriteItem favourite, CancellationToken cancellationToken = default)
    {
        if (items.All(item => item.ItemId != favourite.ItemId))
            items.Add(favourite);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int itemId, CancellationToken cancellationToken = default)
    {
        items.RemoveAll(item => item.ItemId == itemId);
        return Task.CompletedTask;
    }
}
