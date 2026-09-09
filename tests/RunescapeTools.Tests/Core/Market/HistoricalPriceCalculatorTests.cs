namespace RunescapeTools.Tests.Core.Market;

[TestFixture]
public sealed class HistoricalPriceCalculatorTests
{
    internal static readonly DateTimeOffset End = new(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);
    internal static PricePoint[] History() => Enumerable.Range(1, 120)
        .Select(i => new PricePoint(End.AddHours(-6 * i), 2, 1, 10, 10)).ToArray();

    [Test]
    public void SidesAreVolumeWeightedSeparatelyWithoutIntegerRounding()
    {
        var history = History();
        history[0] = history[0] with { AverageHigh = 4, HighVolume = 30, AverageLow = 3, LowVolume = 20 };
        var result = HistoricalPriceCalculator.Calculate(1, history, End);
        Assert.That(result.High, Is.EqualTo((119 * 20m + 120m) / 1220m));
        Assert.That(result.Low, Is.EqualTo((119 * 10m + 60m) / 1210m));
        Assert.That(result.Basis, Is.EqualTo(PricingMode.ThirtyDayAverage));
    }

    [Test]
    public void OldAndUncompletedBucketsAndDuplicateTimestampsDoNotInflateAverage()
    {
        var history = History();
        var result = HistoricalPriceCalculator.Calculate(1,
            history.Concat(history).Append(new(End, 99999, 99999, 10000, 10000))
                .Append(new(End.AddDays(-31), 99999, 99999, 10000, 10000)), End);
        Assert.That(result.High, Is.EqualTo(2m));
        Assert.That(result.Low, Is.EqualTo(1m));
    }

    [Test]
    public void ShortHistoryRemainsUnpriced()
    {
        var result = HistoricalPriceCalculator.Calculate(1, History().Take(115), End);
        Assert.That(result.High, Is.Null);
        Assert.That(result.Low, Is.Null);
    }

    [Test]
    public void MissingSideDoesNotUseOtherSideAndZeroVolumeIsIgnored()
    {
        var history = History().Select(p => p with { HighVolume = 0 }).ToArray();
        var result = HistoricalPriceCalculator.Calculate(1, history, End);
        Assert.That(TrainingMarketPricing.Select(TrainingFlowDirection.Input, result).UnitPrice, Is.Null);
        Assert.That(TrainingMarketPricing.Select(TrainingFlowDirection.Output, result).UnitPrice, Is.EqualTo(1));
        Assert.That(HistoricalPriceCalculator.Calculate(1,
            history.Select(p => p with { AverageHigh = null, HighVolume = 100 }), End).High, Is.Null);
    }
}
