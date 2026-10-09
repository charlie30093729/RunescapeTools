namespace RunescapeTools.Tests.Application.Market;

[TestFixture]
[Category("Unit")]
public sealed class MarketDataServiceTests
{
    [Test]
    [Property("LegacyScenario", "LatestPricesAreCached")]
    [Description("latest prices are cached and missing prices are omitted")]
    public async Task LatestPricesAreCached()
    {
        var client = new FakePriceClient { Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100) } };
        var service = CreateMarketService(client);

        var first = await service.GetLatestForAsync([1, 2]);
        var second = await service.GetLatestForAsync([1]);

        Assert.That(first.ContainsKey(1), Is.True, "known price should be present");
        Assert.That(!first.ContainsKey(2), Is.True, "missing price should be omitted");
        Assert.That(client.LatestCalls, Is.EqualTo(1), "latest API call count");
        Assert.That(second[1].MidPrice ?? 0, Is.EqualTo(100m).Within(0m), "cached quote");
    }

    [Test]
    [Property("LegacyScenario", "HistoryWindowsAreFilteredAndCached")]
    [Description("history windows are filtered and cached by resolution")]
    public async Task HistoryWindowsAreFilteredAndCached()
    {
        var now = new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);
        var client = new FakePriceClient
        {
            HistoryByTimeStep = new Dictionary<PriceTimeStep, IReadOnlyList<PricePoint>>
            {
                [PriceTimeStep.OneHour] =
                [
                    Point(now.AddDays(-8), 10),
                    Point(now.AddDays(-6), 20),
                    Point(now.AddHours(-1), 30)
                ],
                [PriceTimeStep.SixHours] =
                [
                    Point(now.AddDays(-31), 40),
                    Point(now.AddDays(-29), 50),
                    Point(now.AddHours(-6), 60)
                ]
            }
        };
        var service = CreateMarketService(client, now);

        var weeklyFirst = await service.GetWeeklyHistoryAsync(1);
        var weeklySecond = await service.GetWeeklyHistoryAsync(1);
        var monthlyFirst = await service.GetHistoryAsync(
            1,
            PriceTimeStep.SixHours,
            TimeSpan.FromDays(30));
        var monthlySecond = await service.GetHistoryAsync(
            1,
            PriceTimeStep.SixHours,
            TimeSpan.FromDays(30));

        Assert.That(weeklyFirst.Count, Is.EqualTo(2), "filtered weekly history count");
        Assert.That(weeklySecond.Count, Is.EqualTo(weeklyFirst.Count), "cached weekly history count");
        Assert.That(monthlyFirst.Count, Is.EqualTo(2), "filtered monthly history count");
        Assert.That(monthlySecond.Count, Is.EqualTo(monthlyFirst.Count), "cached monthly history count");
        Assert.That(client.HistoryCalls, Is.EqualTo(2), "one API call per history resolution");
        Assert.That(client.HistoryTimeSteps.SequenceEqual(
                [PriceTimeStep.OneHour, PriceTimeStep.SixHours]), Is.True, "history resolutions are cached independently");
    }

    [Test]
    [Property("LegacyScenario", "SearchOrdering")]
    [Description("search favours prefix matches and respects limits")]
    public async Task SearchOrdering()
    {
        var client = new FakePriceClient
        {
            Mapping =
            [
                Map(1, "Rune platebody"),
                Map(2, "Broken rune plate"),
                Map(3, "Rune bar"),
                Map(4, "Runite ore")
            ]
        };
        var service = CreateMarketService(client);

        var results = await service.SearchItemsAsync("rune", 3);

        Assert.That(results.Count, Is.EqualTo(3), "search limit");
        Assert.That(results[0].Name, Is.EqualTo("Rune bar"), "shortest prefix match");
        Assert.That(results[1].Name, Is.EqualTo("Rune platebody"), "second prefix match");
        Assert.That(client.MappingCalls, Is.EqualTo(1), "mapping cache count");
    }

    private static MarketDataService CreateMarketService(FakePriceClient client, DateTimeOffset? now = null) => new(
        client,
        new MarketDataOptions
        {
            LatestCacheDuration = TimeSpan.FromMinutes(5),
            MappingCacheDuration = TimeSpan.FromHours(1),
            HistoryCacheDuration = TimeSpan.FromMinutes(5),
            HistoryWindow = TimeSpan.FromDays(7)
        },
        new TestTimeProvider(now ?? new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero)));
}
