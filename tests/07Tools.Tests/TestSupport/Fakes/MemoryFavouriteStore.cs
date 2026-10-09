namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class MemoryFavouriteStore(params FavouriteItem[] initial) : IFavouriteStore
{
    private readonly List<FavouriteItem> items = [.. initial];
    public Exception? MoveFailure { get; set; }
    public Task? MoveDelay { get; set; }

    public Task<IReadOnlyList<FavouriteItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FavouriteItem>>(items.ToArray());

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

    public async Task MoveBeforeAsync(int itemId, int? beforeItemId, CancellationToken cancellationToken = default)
    {
        if (MoveDelay is not null)
            await MoveDelay.WaitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (MoveFailure is not null)
            throw MoveFailure;

        var favourite = items.Single(item => item.ItemId == itemId);
        if (beforeItemId == itemId)
            return;
        var anchor = beforeItemId.HasValue
            ? items.Single(item => item.ItemId == beforeItemId.Value)
            : null;
        items.Remove(favourite);
        items.Insert(anchor is null ? items.Count : items.IndexOf(anchor), favourite);
    }
}
