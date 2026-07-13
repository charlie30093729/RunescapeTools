namespace RunescapePriceChecker.Core.Market;

public enum PriceTimeStep
{
    FiveMinutes,
    OneHour,
    SixHours,
    TwentyFourHours
}

public sealed record ItemMapping(
    int Id,
    string Name,
    string Examine,
    bool Members,
    int? BuyLimit,
    string Icon);

public sealed record ItemPrice(
    int ItemId,
    long? High,
    long? Low,
    DateTimeOffset? HighTime,
    DateTimeOffset? LowTime)
{
    public decimal? MidPrice => (High, Low) switch
    {
        (not null, not null) => (High.Value + Low.Value) / 2m,
        (not null, null) => High.Value,
        (null, not null) => Low.Value,
        _ => null
    };

    public DateTimeOffset? LastUpdated => HighTime > LowTime ? HighTime : LowTime;
}

public sealed record PricePoint(
    DateTimeOffset Timestamp,
    long? AverageHigh,
    long? AverageLow,
    long HighVolume,
    long LowVolume)
{
    public decimal? MidPrice => (AverageHigh, AverageLow) switch
    {
        (not null, not null) => (AverageHigh.Value + AverageLow.Value) / 2m,
        (not null, null) => AverageHigh.Value,
        (null, not null) => AverageLow.Value,
        _ => null
    };
}
