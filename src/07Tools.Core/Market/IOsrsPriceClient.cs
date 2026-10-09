namespace RunescapeTools.Core.Market;

public interface IOsrsPriceClient
{
    Task<IReadOnlyList<ItemMapping>> GetMappingAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PricePoint>> GetTimeSeriesAsync(
        int itemId,
        PriceTimeStep timeStep,
        CancellationToken cancellationToken = default);
}
