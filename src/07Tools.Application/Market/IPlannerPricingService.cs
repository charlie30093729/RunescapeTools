using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;

namespace RunescapeTools.Application.Market;

public sealed record CachedPriceHistory(DateTimeOffset FetchedAt, IReadOnlyList<PricePoint> Points);

public interface IPriceHistoryStore
{
    Task<CachedPriceHistory?> ReadAsync(int itemId, CancellationToken cancellationToken);
    Task WriteAsync(int itemId, CachedPriceHistory history, CancellationToken cancellationToken);
    Task<PricingMode> ReadModeAsync(CancellationToken cancellationToken);
    Task WriteModeAsync(PricingMode mode, CancellationToken cancellationToken);
}

public sealed record PlannerPriceSnapshot(
    PricingMode Mode, DateTimeOffset AsOf, IReadOnlyDictionary<int, ItemPrice> Prices,
    decimal? MoneyMakerProfitPerHour, bool MoneyMakerUnpriced, int IncompleteItems);

public interface IPlannerPricingService
{
    Task<PricingMode> ReadModeAsync(CancellationToken cancellationToken = default);
    Task SaveModeAsync(PricingMode mode, CancellationToken cancellationToken = default);
    Task<PlannerPriceSnapshot> GetAsync(IEnumerable<int> itemIds, PricingMode mode,
        MoneyMakingMethodDefinition? moneyMaker = null, CancellationToken cancellationToken = default);
}
