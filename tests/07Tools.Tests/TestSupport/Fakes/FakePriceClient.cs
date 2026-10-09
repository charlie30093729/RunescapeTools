namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class FakePriceClient : IOsrsPriceClient
{
    public IReadOnlyList<ItemMapping> Mapping { get; init; } = [];
    public IReadOnlyDictionary<int, ItemPrice> Latest { get; init; } = new Dictionary<int, ItemPrice>();
    public IReadOnlyList<PricePoint> History { get; init; } = [];
    public IReadOnlyDictionary<PriceTimeStep, IReadOnlyList<PricePoint>> HistoryByTimeStep { get; init; } =
        new Dictionary<PriceTimeStep, IReadOnlyList<PricePoint>>();
    public int MappingCalls { get; private set; }
    public int LatestCalls { get; private set; }
    public int HistoryCalls { get; private set; }
    public List<PriceTimeStep> HistoryTimeSteps { get; } = [];

    public Task<IReadOnlyList<ItemMapping>> GetMappingAsync(CancellationToken cancellationToken = default)
    {
        MappingCalls++;
        return Task.FromResult(Mapping);
    }

    public Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        LatestCalls++;
        return Task.FromResult(Latest);
    }

    public Task<IReadOnlyList<PricePoint>> GetTimeSeriesAsync(int itemId, PriceTimeStep timeStep, CancellationToken cancellationToken = default)
    {
        HistoryCalls++;
        HistoryTimeSteps.Add(timeStep);
        return Task.FromResult(
            HistoryByTimeStep.TryGetValue(timeStep, out var history)
                ? history
                : History);
    }
}
