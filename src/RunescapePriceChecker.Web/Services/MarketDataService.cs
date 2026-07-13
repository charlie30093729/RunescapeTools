using RunescapePriceChecker.Core.Market;

namespace RunescapePriceChecker.Web.Services;

public sealed class MarketDataService(IOsrsPriceClient client)
{
    private static readonly TimeSpan LatestCacheDuration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MappingCacheDuration = TimeSpan.FromHours(12);
    private static readonly TimeSpan HistoryCacheDuration = TimeSpan.FromMinutes(15);

    private readonly SemaphoreSlim latestGate = new(1, 1);
    private readonly SemaphoreSlim mappingGate = new(1, 1);
    private readonly SemaphoreSlim historyGate = new(1, 1);
    private IReadOnlyDictionary<int, ItemPrice>? latest;
    private DateTimeOffset latestFetchedAt;
    private IReadOnlyList<ItemMapping>? mapping;
    private DateTimeOffset mappingFetchedAt;
    private readonly Dictionary<int, (DateTimeOffset FetchedAt, IReadOnlyList<PricePoint> Points)> weeklyHistory = [];

    public async Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestForAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        var wantedIds = itemIds.Distinct().ToArray();

        await latestGate.WaitAsync(cancellationToken);
        try
        {
            if (latest is null || DateTimeOffset.UtcNow - latestFetchedAt > LatestCacheDuration)
            {
                latest = await client.GetLatestAsync(cancellationToken);
                latestFetchedAt = DateTimeOffset.UtcNow;
            }

            return wantedIds
                .Where(latest.ContainsKey)
                .ToDictionary(id => id, id => latest[id]);
        }
        finally
        {
            latestGate.Release();
        }
    }

    public async Task<IReadOnlyList<ItemMapping>> SearchItemsAsync(
        string query,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return [];

        var items = await GetMappingAsync(cancellationToken);
        var term = query.Trim();
        return items
            .Where(item => item.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(item => item.Name.Length)
            .ThenBy(item => item.Name)
            .Take(take)
            .ToArray();
    }

    public async Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default)
    {
        await historyGate.WaitAsync(cancellationToken);
        try
        {
            if (!weeklyHistory.TryGetValue(itemId, out var cached)
                || DateTimeOffset.UtcNow - cached.FetchedAt > HistoryCacheDuration)
            {
                var points = await client.GetTimeSeriesAsync(itemId, PriceTimeStep.OneHour, cancellationToken);
                var cutoff = DateTimeOffset.UtcNow.AddDays(-7);
                cached = (DateTimeOffset.UtcNow, points.Where(point => point.Timestamp >= cutoff).ToArray());
                weeklyHistory[itemId] = cached;
            }

            return cached.Points;
        }
        finally
        {
            historyGate.Release();
        }
    }

    private async Task<IReadOnlyList<ItemMapping>> GetMappingAsync(CancellationToken cancellationToken)
    {
        await mappingGate.WaitAsync(cancellationToken);
        try
        {
            if (mapping is null || DateTimeOffset.UtcNow - mappingFetchedAt > MappingCacheDuration)
            {
                mapping = await client.GetMappingAsync(cancellationToken);
                mappingFetchedAt = DateTimeOffset.UtcNow;
            }

            return mapping;
        }
        finally
        {
            mappingGate.Release();
        }
    }
}
