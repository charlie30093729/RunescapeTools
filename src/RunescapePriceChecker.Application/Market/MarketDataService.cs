using System.Collections.Concurrent;
using RunescapePriceChecker.Core.Market;

namespace RunescapePriceChecker.Application.Market;

public sealed class MarketDataService(
    IOsrsPriceClient client,
    MarketDataOptions options,
    TimeProvider? timeProvider = null) : IMarketDataService
{
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;
    private readonly SemaphoreSlim latestGate = new(1, 1);
    private readonly SemaphoreSlim mappingGate = new(1, 1);
    private readonly ConcurrentDictionary<int, SemaphoreSlim> historyGates = new();
    private readonly ConcurrentDictionary<int, HistoryCacheEntry> weeklyHistory = new();
    private IReadOnlyDictionary<int, ItemPrice>? latest;
    private DateTimeOffset latestFetchedAt;
    private IReadOnlyList<ItemMapping>? mapping;
    private DateTimeOffset mappingFetchedAt;

    public async Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestForAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var wantedIds = itemIds.Distinct().ToArray();
        if (wantedIds.Length == 0)
            return new Dictionary<int, ItemPrice>();

        await latestGate.WaitAsync(cancellationToken);
        try
        {
            var now = clock.GetUtcNow();
            if (latest is null || now - latestFetchedAt > options.LatestCacheDuration)
            {
                latest = await client.GetLatestAsync(cancellationToken);
                latestFetchedAt = now;
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
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2 || take <= 0)
            return [];

        var items = await GetMappingAsync(cancellationToken);
        var term = query.Trim();
        return items
            .Where(item => item.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(item => item.Name.Length)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(take)
            .ToArray();
    }

    public async Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default)
    {
        if (itemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemId));

        var gate = historyGates.GetOrAdd(itemId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var now = clock.GetUtcNow();
            if (!weeklyHistory.TryGetValue(itemId, out var cached)
                || now - cached.FetchedAt > options.HistoryCacheDuration)
            {
                var points = await client.GetTimeSeriesAsync(itemId, PriceTimeStep.OneHour, cancellationToken);
                var cutoff = now - options.HistoryWindow;
                cached = new HistoryCacheEntry(
                    now,
                    points.Where(point => point.Timestamp >= cutoff)
                        .OrderBy(point => point.Timestamp)
                        .ToArray());
                weeklyHistory[itemId] = cached;
            }

            return cached.Points;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<IReadOnlyList<ItemMapping>> GetMappingAsync(CancellationToken cancellationToken)
    {
        await mappingGate.WaitAsync(cancellationToken);
        try
        {
            var now = clock.GetUtcNow();
            if (mapping is null || now - mappingFetchedAt > options.MappingCacheDuration)
            {
                mapping = await client.GetMappingAsync(cancellationToken);
                mappingFetchedAt = now;
            }

            return mapping;
        }
        finally
        {
            mappingGate.Release();
        }
    }

    private sealed record HistoryCacheEntry(
        DateTimeOffset FetchedAt,
        IReadOnlyList<PricePoint> Points);
}
