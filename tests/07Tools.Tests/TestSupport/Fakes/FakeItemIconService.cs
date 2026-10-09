namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class FakeItemIconService(params ItemIcon[] icons) : IItemIconService
{
    private readonly IReadOnlyDictionary<int, ItemIcon> icons = icons.ToDictionary(icon => icon.ItemId);
    public List<int> RequestedItemIds { get; } = [];

    public Task<ItemIcon?> GetAsync(int itemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(icons.GetValueOrDefault(itemId));

    public Task<IReadOnlyDictionary<int, ItemIcon>> GetManyAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        var requested = itemIds.ToHashSet();
        RequestedItemIds.AddRange(requested);
        return Task.FromResult<IReadOnlyDictionary<int, ItemIcon>>(
            icons.Where(pair => requested.Contains(pair.Key)).ToDictionary());
    }
}
