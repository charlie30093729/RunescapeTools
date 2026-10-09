using static RunescapeTools.Tests.Core.Market.HistoricalPriceCalculatorTests;

namespace RunescapeTools.Tests.Application.Market;

[TestFixture]
public sealed class PlannerPricingServiceTests
{
    [Test]
    public async Task DeduplicatesReusesCacheAcrossTogglesAndSurvivesServiceRestart()
    {
        using var folder = new TemporaryDirectory();
        var market = new FakeMarketDataService { History = History(), Latest = new Dictionary<int, ItemPrice> { [1] = new(1, 5, 4, End, End) } };
        var store = new JsonPriceHistoryStore(new(folder.Path));
        var clock = new TestTimeProvider(End.AddMinutes(10));
        var service = new PlannerPricingService(market, store, clock);
        var average = await service.GetAsync([1, 1], PricingMode.ThirtyDayAverage);
        Assert.That(average.Prices[1].High, Is.EqualTo(2));
        var live = await service.GetAsync([1], PricingMode.Live);
        Assert.That(live.Prices[1].High, Is.EqualTo(5));
        await service.GetAsync([1], PricingMode.ThirtyDayAverage);
        await new PlannerPricingService(market, store, clock).GetAsync([1], PricingMode.ThirtyDayAverage);
        Assert.That(market.HistoryRequests, Is.EqualTo(new[] { 1 }));
        Assert.That(market.HistoryTimeSteps.Single(), Is.EqualTo(PriceTimeStep.SixHours));
        await service.SaveModeAsync(PricingMode.ThirtyDayAverage);
        Assert.That(await new PlannerPricingService(market, store, clock).ReadModeAsync(), Is.EqualTo(PricingMode.ThirtyDayAverage));
    }

    [Test]
    public async Task ExpiredCacheIsRefetchedAndInvalidIdsAreIgnored()
    {
        using var folder = new TemporaryDirectory();
        var store = new JsonPriceHistoryStore(new(folder.Path));
        await store.WriteAsync(1, new(End.AddHours(-7), History()), default);
        var market = new FakeMarketDataService { History = History() };
        await new PlannerPricingService(market, store, new TestTimeProvider(End)).GetAsync([0, -1, 1], PricingMode.ThirtyDayAverage);
        Assert.That(market.HistoryRequests, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public async Task MoneyMakerUsesSamePricesEffectiveActionRateAccountsAndTax()
    {
        using var folder = new TemporaryDirectory();
        var service = new PlannerPricingService(new FakeMarketDataService { History = History() },
            new JsonPriceHistoryStore(new(folder.Path)), new TestTimeProvider(End));
        var method = new MoneyMakingMethodDefinition("test", "Test", "", 88, 5, 0.02m,
            [new(1, "Input", 1, ItemFlowDirection.Input, QuantityBasis.PerAction),
             new(2, "Output", 4, ItemFlowDirection.Output, QuantityBasis.PerAction)]);
        var result = await service.GetAsync([1], PricingMode.ThirtyDayAverage, method);
        Assert.That(result.Prices.Keys, Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(result.MoneyMakerProfitPerHour, Is.EqualTo((4 * 1m * 0.98m - 2m) * 88 * 5));
        Assert.That(result.MoneyMakerUnpriced, Is.False);
    }

    [Test]
    public async Task IncompleteHistoryExcludesMoneyMakerInsteadOfCreditingPartialProfit()
    {
        using var folder = new TemporaryDirectory();
        var service = new PlannerPricingService(new FakeMarketDataService(),
            new JsonPriceHistoryStore(new(folder.Path)), new TestTimeProvider(End));
        var method = new MoneyMakingMethodDefinition("test", "Test", "", 100, 5, 0.02m,
            [new(1, "Output", 1, ItemFlowDirection.Output)]);
        var result = await service.GetAsync([1], PricingMode.ThirtyDayAverage, method);
        Assert.That(result.IncompleteItems, Is.EqualTo(1));
        Assert.That(result.MoneyMakerProfitPerHour, Is.Null);
        Assert.That(result.MoneyMakerUnpriced, Is.True);
    }

    [Test]
    public void CancellationIsNotReportedAsSuccessfulEmptySnapshot()
    {
        using var folder = new TemporaryDirectory();
        var service = new PlannerPricingService(new FakeMarketDataService(),
            new JsonPriceHistoryStore(new(folder.Path)), new TestTimeProvider(End));
        using var token = new CancellationTokenSource();
        token.Cancel();
        Assert.That((Func<Task>)(async () =>
        {
            await service.GetAsync([1], PricingMode.ThirtyDayAverage, cancellationToken: token.Token);
        }), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task RequestsAreBoundedAndNetworkFailuresCanBeRetried()
    {
        using var folder = new TemporaryDirectory();
        var clock = new TestTimeProvider(End);
        var client = new ConcurrentClient();
        var market = new MarketDataService(client, new MarketDataOptions(), clock);
        var service = new PlannerPricingService(market, new JsonPriceHistoryStore(new(folder.Path)), clock);
        await service.GetAsync(Enumerable.Range(1, 12), PricingMode.ThirtyDayAverage);
        Assert.That(client.MaximumConcurrent, Is.InRange(1, 4));
        client.Fail = true;
        Assert.That((Func<Task>)(async () => await service.GetAsync([99], PricingMode.ThirtyDayAverage)),
            Throws.InstanceOf<HttpRequestException>());
        client.Fail = false;
        Assert.That((await service.GetAsync([99], PricingMode.ThirtyDayAverage)).Prices[99].High, Is.EqualTo(2));
    }

    private sealed class ConcurrentClient : IOsrsPriceClient
    {
        private int current;
        private int maximum;
        public int MaximumConcurrent => maximum;
        public bool Fail { get; set; }
        public Task<IReadOnlyList<ItemMapping>> GetMappingAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public async Task<IReadOnlyList<PricePoint>> GetTimeSeriesAsync(int itemId, PriceTimeStep timeStep,
            CancellationToken cancellationToken = default)
        {
            var active = Interlocked.Increment(ref current);
            int old;
            do { old = maximum; } while (active > old && Interlocked.CompareExchange(ref maximum, active, old) != old);
            try
            {
                await Task.Delay(20, cancellationToken);
                if (Fail) throw new HttpRequestException("Temporary test outage");
                return History();
            }
            finally { Interlocked.Decrement(ref current); }
        }
    }
}
