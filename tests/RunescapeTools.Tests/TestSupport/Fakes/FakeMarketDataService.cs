namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class FakeMarketDataService : IMarketDataService
{
    public IReadOnlyDictionary<int, ItemPrice> Latest { get; init; } = new Dictionary<int, ItemPrice>();
    public IReadOnlyList<ItemMapping> SearchResults { get; init; } = [];
    public IReadOnlyList<ItemMapping> Mappings { get; init; } = [];
    public IReadOnlyList<PricePoint> History { get; init; } = [];
    public IReadOnlyDictionary<PriceTimeStep, IReadOnlyList<PricePoint>> HistoryByTimeStep { get; init; } =
        new Dictionary<PriceTimeStep, IReadOnlyList<PricePoint>>();
    public List<int> HistoryRequests { get; } = [];
    public List<int[]> LatestRequests { get; } = [];
    public List<PriceTimeStep> HistoryTimeSteps { get; } = [];
    public List<TimeSpan> HistoryWindows { get; } = [];
    public Exception? Failure { get; set; }

    public Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestForAsync(IEnumerable<int> itemIds, CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
            throw Failure;
        var requested = itemIds.ToHashSet();
        LatestRequests.Add(requested.ToArray());
        return Task.FromResult<IReadOnlyDictionary<int, ItemPrice>>(
            Latest.Where(pair => requested.Contains(pair.Key)).ToDictionary());
    }

    public Task<IReadOnlyList<ItemMapping>> SearchItemsAsync(string query, int take = 8, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ItemMapping>>(SearchResults.Take(take).ToArray());

    public Task<IReadOnlyDictionary<int, ItemMapping>> GetItemMappingsAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        var requested = itemIds.ToHashSet();
        return Task.FromResult<IReadOnlyDictionary<int, ItemMapping>>(
            Mappings.Where(item => requested.Contains(item.Id)).ToDictionary(item => item.Id));
    }

    public Task<IReadOnlyList<PricePoint>> GetHistoryAsync(
        int itemId,
        PriceTimeStep timeStep,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        HistoryRequests.Add(itemId);
        HistoryTimeSteps.Add(timeStep);
        HistoryWindows.Add(window);
        return Task.FromResult(
            HistoryByTimeStep.TryGetValue(timeStep, out var history)
                ? history
                : History);
    }

    public Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default) =>
        GetHistoryAsync(
            itemId,
            PriceTimeStep.OneHour,
            TimeSpan.FromDays(7),
            cancellationToken);
}
