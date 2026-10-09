namespace RunescapeTools.Tests.Application.Favourites;

[TestFixture]
[Category("Unit")]
public sealed class FavouriteHistoryWarmupTests
{
    [Test]
    [Property("LegacyScenario", "FavouriteWarmup")]
    [Description("favourite warmup requests every saved history")]
    public async Task FavouriteWarmup()
    {
        var store = new MemoryFavouriteStore(
            new FavouriteItem(1, "One", DateTimeOffset.UtcNow),
            new FavouriteItem(2, "Two", DateTimeOffset.UtcNow));
        var market = new FakeMarketDataService();
        var warmup = new FavouriteHistoryWarmupService(store, market, new MarketDataOptions { WarmupConcurrency = 1 });

        await warmup.WarmAsync();

        Assert.That(market.HistoryRequests.Count, Is.EqualTo(2), "warmup request count");
        Assert.That(market.HistoryRequests.Order().SequenceEqual([1, 2]), Is.True, "warmup item ids");
    }
}
