using System.Collections.Concurrent;
using RunescapeTools.Core.Market;

namespace RunescapeTools.Application.Market;

public sealed class MarketDataService(
    IOsrsPriceClient client,
    MarketDataOptions options,
    TimeProvider? timeProvider = null) : IMarketDataService
{
    private readonly TimeProvider clock = timeProvider ?? TimeProvider.System;
    private readonly SemaphoreSlim latestGate = new(1, 1);
    private readonly SemaphoreSlim mappingGate = new(1, 1);
    private readonly ConcurrentDictionary<HistoryCacheKey, SemaphoreSlim> historyGates = new();
    private readonly ConcurrentDictionary<HistoryCacheKey, HistoryCacheEntry> history = new();
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

    public async Task<IReadOnlyDictionary<int, ItemMapping>> GetItemMappingsAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var wantedIds = itemIds.Where(id => id > 0).Distinct().ToHashSet();
        if (wantedIds.Count == 0)
            return new Dictionary<int, ItemMapping>();

        var items = await GetMappingAsync(cancellationToken);
        return items
            .Where(item => wantedIds.Contains(item.Id))
            .ToDictionary(item => item.Id);
    }

    public async Task<IReadOnlyList<PricePoint>> GetHistoryAsync(
        int itemId,
        PriceTimeStep timeStep,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        if (itemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemId));
        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window));

        var key = new HistoryCacheKey(itemId, timeStep);
        var gate = historyGates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var now = clock.GetUtcNow();
            if (!history.TryGetValue(key, out var cached)
                || now - cached.FetchedAt > options.HistoryCacheDuration)
            {
                var points = await client.GetTimeSeriesAsync(itemId, timeStep, cancellationToken);
                cached = new HistoryCacheEntry(
                    now,
                    points.OrderBy(point => point.Timestamp).ToArray());
                history[key] = cached;
            }

            var cutoff = now - window;
            return cached.Points
                .Where(point => point.Timestamp >= cutoff && point.Timestamp <= now)
                .ToArray();
        }
        finally
        {
            gate.Release();
        }
    }

    public Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default) =>
        GetHistoryAsync(
            itemId,
            PriceTimeStep.OneHour,
            options.HistoryWindow,
            cancellationToken);

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

    private readonly record struct HistoryCacheKey(
        int ItemId,
        PriceTimeStep TimeStep);
}
