using RunescapePriceChecker.Core.Market;

namespace RunescapePriceChecker.Application.Market;

public interface IMarketDataService
{
    Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestForAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemMapping>> SearchItemsAsync(
        string query,
        int take = 8,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default);
}
