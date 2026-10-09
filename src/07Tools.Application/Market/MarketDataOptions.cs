namespace RunescapeTools.Application.Market;

public sealed class MarketDataOptions
{
    public TimeSpan LatestCacheDuration { get; init; } = TimeSpan.FromMinutes(1);

    public TimeSpan MappingCacheDuration { get; init; } = TimeSpan.FromHours(12);

    public TimeSpan HistoryCacheDuration { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan HistoryWindow { get; init; } = TimeSpan.FromDays(7);

    public int WarmupConcurrency { get; init; } = 3;
}
