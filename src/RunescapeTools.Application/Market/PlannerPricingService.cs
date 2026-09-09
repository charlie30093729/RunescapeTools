using System.Collections.Concurrent;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;

namespace RunescapeTools.Application.Market;

public sealed class PlannerPricingService(
    IMarketDataService market, IPriceHistoryStore store, TimeProvider clock,
    MoneyMakingCalculator? calculator = null) : IPlannerPricingService
{
    private readonly MoneyMakingCalculator moneyMakingCalculator = calculator ?? new();
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Dictionary<int, CachedPriceHistory> memory = [];

    public Task<PricingMode> ReadModeAsync(CancellationToken cancellationToken = default) =>
        store.ReadModeAsync(cancellationToken);
    public Task SaveModeAsync(PricingMode mode, CancellationToken cancellationToken = default) =>
        store.WriteModeAsync(mode, cancellationToken);

    public async Task<PlannerPriceSnapshot> GetAsync(IEnumerable<int> itemIds, PricingMode mode,
        MoneyMakingMethodDefinition? moneyMaker = null, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        var ids = itemIds.Concat(moneyMaker?.RequiredItemIds ?? []).Where(id => id > 0).Distinct().ToArray();
        await gate.WaitAsync(cancellationToken);
        try
        {
            var now = clock.GetUtcNow();
            var end = HistoricalPriceCalculator.WindowEnd(now);
            IReadOnlyDictionary<int, ItemPrice> prices;
            if (mode == PricingMode.Live)
                prices = await market.GetLatestForAsync(ids, cancellationToken);
            else
            {
                var histories = new ConcurrentDictionary<int, CachedPriceHistory>();
                await Parallel.ForEachAsync(ids,
                    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cancellationToken },
                    async (id, token) =>
                    {
                        var cached = memory.GetValueOrDefault(id) ?? await store.ReadAsync(id, token);
                        if (cached is null || cached.FetchedAt < end || cached.FetchedAt > now)
                        {
                            var points = await market.GetHistoryAsync(id, PriceTimeStep.SixHours,
                                TimeSpan.FromDays(31), token);
                            cached = new CachedPriceHistory(now, points);
                            await store.WriteAsync(id, cached, token);
                        }
                        histories[id] = cached;
                    });
                foreach (var pair in histories) memory[pair.Key] = pair.Value;
                prices = histories.ToDictionary(p => p.Key,
                    p => HistoricalPriceCalculator.Calculate(p.Key, p.Value.Points, end));
            }
            cancellationToken.ThrowIfCancellationRequested();
            MoneyMakingResult? result = moneyMaker is null ? null :
                moneyMakingCalculator.Calculate(moneyMaker, prices, useTradeSides: true);
            var incomplete = ids.Count(id => !prices.TryGetValue(id, out var p) || p.High is null || p.Low is null);
            return new(mode, mode == PricingMode.Live ? now : end, prices,
                result is { HasMissingPrices: false } ? result.ProfitAllAccounts : null,
                result?.HasMissingPrices ?? false, incomplete);
        }
        finally { gate.Release(); }
    }
}
