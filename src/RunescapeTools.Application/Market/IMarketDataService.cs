using RunescapeTools.Core.Market;

namespace RunescapeTools.Application.Market;

public interface IMarketDataService
{
    Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestForAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemMapping>> SearchItemsAsync(
        string query,
        int take = 8,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, ItemMapping>> GetItemMappingsAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PricePoint>> GetHistoryAsync(
        int itemId,
        PriceTimeStep timeStep,
        TimeSpan window,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default);
}
