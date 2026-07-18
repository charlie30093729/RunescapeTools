using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RunescapeTools.Application.Favourites;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Favourites;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;
using RunescapeTools.Core.MoneyMaking.Methods;
using RunescapeTools.Infrastructure.Configuration;
using RunescapeTools.Infrastructure.Market;
using RunescapeTools.Infrastructure.Persistence;
using RunescapeTools.Wpf.ViewModels;

var tests = new (string Name, Func<Task> Run)[]
{
    ("generic flow calculation", () => RunSync(GenericFlowCalculation)),
    ("Vyrewatch matches the legacy formula", () => RunSync(VyrewatchMatchesLegacyFormula)),
    ("Vyrewatch exposes every required item once", () => RunSync(VyrewatchItemIdsAreDistinct)),
    ("mid price falls back to the available quote", () => RunSync(MidPriceFallback)),
    ("latest prices are cached and missing prices are omitted", LatestPricesAreCached),
    ("weekly history is filtered and cached", WeeklyHistoryIsFilteredAndCached),
    ("search favours prefix matches and respects limits", SearchOrdering),
    ("favourite warmup requests every saved history", FavouriteWarmup),
    ("Wiki client retries transient responses", WikiClientRetries),
    ("JSON store seeds, sorts, and prevents duplicates", JsonStoreSeedsSortsAndDeduplicates),
    ("JSON store never overwrites existing state", JsonStoreDoesNotOverwrite),
    ("dashboard view-model loads and reports failures", DashboardViewModelStates),
    ("favourites view-model searches, adds, selects, and removes", FavouritesViewModelFlow),
    ("money-maker view-model reprices the complete ledger", MoneyMakerViewModelFlow),
    ("shell navigation loads the requested page", ShellNavigation)
};

var failures = new List<string>();
foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS  {test.Name}");
    }
    catch (Exception exception)
    {
        failures.Add(test.Name);
        Console.WriteLine($"FAIL  {test.Name}: {exception.Message}");
    }
}

Console.WriteLine();
Console.WriteLine($"{tests.Length - failures.Count}/{tests.Length} checks passed.");
return failures.Count == 0 ? 0 : 1;

static Task RunSync(Action action)
{
    action();
    return Task.CompletedTask;
}

static void GenericFlowCalculation()
{
    var method = new MoneyMakingMethodDefinition(
        "test", "Test", "Known values", 10m, 2, 0.02m,
        [
            new ItemFlow(1, "Input", 3m, ItemFlowDirection.Input),
            new ItemFlow(2, "Output", 0.5m, ItemFlowDirection.Output, QuantityBasis.PerAction)
        ]);
    var prices = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100), [2] = Quote(2, 1000) };

    var result = new MoneyMakingCalculator().Calculate(method, prices);

    EqualDecimal(5_000m, result.GrossRevenuePerAccount, "gross revenue");
    EqualDecimal(100m, result.TaxPerAccount, "tax");
    EqualDecimal(300m, result.InputCostPerAccount, "input cost");
    EqualDecimal(4_600m, result.ProfitPerAccount, "profit per account");
    EqualDecimal(9_200m, result.ProfitAllAccounts, "profit for all accounts");
}

static void VyrewatchMatchesLegacyFormula()
{
    var method = new VyrewatchMethod().Definition;
    var prices = method.RequiredItemIds.ToDictionary(id => id, id => Quote(id, 1_000));
    var expectedOutputQuantityPerKill = (1m / 1500m) + (4m / 128m) + (1m / 100m) + (1m / 106m) + (12m / 128m);
    var expectedGross = expectedOutputQuantityPerKill * 102m * 1_000m;
    var expectedSupplies = 4m * 1_000m;
    var expectedProfit = expectedGross * 0.98m - expectedSupplies;

    var result = new MoneyMakingCalculator().Calculate(method, prices);

    EqualDecimal(expectedGross, result.GrossRevenuePerAccount, "legacy gross output", 0.0001m);
    EqualDecimal(expectedSupplies, result.InputCostPerAccount, "legacy hourly supplies");
    EqualDecimal(expectedProfit, result.ProfitPerAccount, "legacy profit", 0.0001m);
    EqualDecimal(expectedProfit * 5m, result.ProfitAllAccounts, "legacy multi-account profit", 0.0001m);
}

static void VyrewatchItemIdsAreDistinct()
{
    var method = new VyrewatchMethod().Definition;
    EqualDecimal(10m, method.RequiredItemIds.Count, "required item count");
    EqualDecimal(10m, method.Items.Select(item => item.ItemId).Distinct().Count(), "unique item count");
}

static void MidPriceFallback()
{
    EqualDecimal(777m, new ItemPrice(1, 777, null, null, null).MidPrice ?? 0, "high-only midpoint");
    EqualDecimal(555m, new ItemPrice(2, null, 555, null, null).MidPrice ?? 0, "low-only midpoint");
}

static async Task LatestPricesAreCached()
{
    var client = new FakePriceClient { Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100) } };
    var service = CreateMarketService(client);

    var first = await service.GetLatestForAsync([1, 2]);
    var second = await service.GetLatestForAsync([1]);

    True(first.ContainsKey(1), "known price should be present");
    True(!first.ContainsKey(2), "missing price should be omitted");
    Equal(1, client.LatestCalls, "latest API call count");
    EqualDecimal(100m, second[1].MidPrice ?? 0, "cached quote");
}

static async Task WeeklyHistoryIsFilteredAndCached()
{
    var now = new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);
    var client = new FakePriceClient
    {
        History =
        [
            Point(now.AddDays(-8), 10),
            Point(now.AddDays(-6), 20),
            Point(now.AddHours(-1), 30)
        ]
    };
    var service = CreateMarketService(client, now);

    var first = await service.GetWeeklyHistoryAsync(1);
    var second = await service.GetWeeklyHistoryAsync(1);

    Equal(2, first.Count, "filtered history count");
    Equal(1, client.HistoryCalls, "history API call count");
    Equal(first.Count, second.Count, "cached history count");
}

static async Task SearchOrdering()
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

    Equal(3, results.Count, "search limit");
    Equal("Rune bar", results[0].Name, "shortest prefix match");
    Equal("Rune platebody", results[1].Name, "second prefix match");
    Equal(1, client.MappingCalls, "mapping cache count");
}

static async Task FavouriteWarmup()
{
    var store = new MemoryFavouriteStore(
        new FavouriteItem(1, "One", DateTimeOffset.UtcNow),
        new FavouriteItem(2, "Two", DateTimeOffset.UtcNow));
    var market = new FakeMarketDataService();
    var warmup = new FavouriteHistoryWarmupService(store, market, new MarketDataOptions { WarmupConcurrency = 1 });

    await warmup.WarmAsync();

    Equal(2, market.HistoryRequests.Count, "warmup request count");
    True(market.HistoryRequests.Order().SequenceEqual([1, 2]), "warmup item ids");
}

static async Task WikiClientRetries()
{
    var handler = new SequenceHandler(
        new HttpResponseMessage(HttpStatusCode.InternalServerError),
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"data\":{\"1\":{\"high\":120,\"low\":100,\"highTime\":1,\"lowTime\":1}}}", Encoding.UTF8, "application/json")
        });
    using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
    var client = new OsrsWikiPriceClient(
        http,
        new OsrsWikiOptions { BaseAddress = http.BaseAddress, MaxRetryAttempts = 2 },
        NullLogger<OsrsWikiPriceClient>.Instance);

    var prices = await client.GetLatestAsync();

    Equal(2, handler.Calls, "HTTP attempts");
    EqualDecimal(110m, prices[1].MidPrice ?? 0, "retried quote midpoint");
}

static async Task JsonStoreSeedsSortsAndDeduplicates()
{
    var directory = CreateTempDirectory();
    try
    {
        var path = Path.Combine(directory, "favourites.json");
        var store = new JsonFavouriteStore(new FavouriteStoreOptions
        {
            FilePath = path,
            SeedJson = "[{\"itemId\":2,\"name\":\"Zulrah scale\",\"addedAt\":\"2026-01-01T00:00:00Z\"},{\"itemId\":1,\"name\":\"Blood shard\",\"addedAt\":\"2026-01-01T00:00:00Z\"}]"
        });

        var seeded = await store.GetAllAsync();
        await store.AddAsync(new FavouriteItem(1, "Duplicate", DateTimeOffset.UtcNow));
        await store.AddAsync(new FavouriteItem(3, "Adamant bar", DateTimeOffset.UtcNow));
        var saved = await store.GetAllAsync();

        Equal("Blood shard", seeded[0].Name, "seed sort order");
        Equal(3, saved.Count, "duplicate prevention");
        Equal("Adamant bar", saved[0].Name, "persisted sort order");
        True(File.Exists(path), "favourites file exists");
        True(!File.Exists(path + ".tmp"), "atomic temporary file is replaced");
    }
    finally
    {
        Directory.Delete(directory, true);
    }
}

static async Task JsonStoreDoesNotOverwrite()
{
    var directory = CreateTempDirectory();
    try
    {
        var path = Path.Combine(directory, "favourites.json");
        await File.WriteAllTextAsync(path, "[{\"itemId\":9,\"name\":\"Existing\",\"addedAt\":\"2026-01-01T00:00:00Z\"}]");
        var store = new JsonFavouriteStore(new FavouriteStoreOptions
        {
            FilePath = path,
            SeedJson = "[{\"itemId\":1,\"name\":\"Seed\",\"addedAt\":\"2026-01-01T00:00:00Z\"}]"
        });

        var items = await store.GetAllAsync();

        Equal(1, items.Count, "existing state count");
        Equal(9, items[0].ItemId, "existing item retained");
    }
    finally
    {
        Directory.Delete(directory, true);
    }
}

static async Task DashboardViewModelStates()
{
    var store = new MemoryFavouriteStore(new FavouriteItem(1, "Rune bar", DateTimeOffset.UtcNow));
    var market = new FakeMarketDataService { Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 500) } };
    var viewModel = new DashboardViewModel(store, market, [new VyrewatchMethod()]);

    await viewModel.LoadAsync();
    Equal(1, viewModel.FavouriteCount, "dashboard favourite count");
    Equal(1, viewModel.Prices.Count, "dashboard price rows");

    market.Failure = new HttpRequestException("offline");
    await viewModel.LoadAsync();
    True(!string.IsNullOrWhiteSpace(viewModel.ErrorMessage), "dashboard error state");
}

static async Task FavouritesViewModelFlow()
{
    var store = new MemoryFavouriteStore(new FavouriteItem(1, "Rune bar", DateTimeOffset.UtcNow));
    var market = new FakeMarketDataService
    {
        Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 500), [2] = Quote(2, 900) },
        SearchResults = [Map(2, "Rune platebody")],
        History = [Point(DateTimeOffset.UtcNow.AddDays(-1), 400), Point(DateTimeOffset.UtcNow, 500)]
    };
    var viewModel = new FavouritesViewModel(store, market, TimeProvider.System);

    await viewModel.LoadAsync();
    viewModel.SearchText = "rune";
    await Task.Delay(350);
    Equal(1, viewModel.SearchResults.Count, "debounced search results");

    await viewModel.AddFavouriteCommand.ExecuteAsync(viewModel.SearchResults[0]);
    Equal(2, viewModel.FavouriteCount, "favourite added");
    Equal(2, viewModel.SelectedFavourite?.ItemId ?? 0, "new favourite selected");

    await viewModel.RemoveFavouriteCommand.ExecuteAsync(viewModel.SelectedFavourite);
    Equal(1, viewModel.FavouriteCount, "favourite removed");
    Equal(1, viewModel.SelectedFavourite?.ItemId ?? 0, "selection moved after removal");
}

static async Task MoneyMakerViewModelFlow()
{
    var method = new VyrewatchMethod();
    var market = new FakeMarketDataService
    {
        Latest = method.Definition.RequiredItemIds.ToDictionary(id => id, id => Quote(id, 1_000))
    };
    var viewModel = new MoneyMakersViewModel([method], new MoneyMakingCalculator(), market);

    await viewModel.LoadAsync();

    Equal(method.Definition.Items.Count, viewModel.FlowRows.Count, "money-making ledger rows");
    True(viewModel.ProfitAllAccounts.EndsWith(" gp", StringComparison.Ordinal), "formatted total profit");
    True(!viewModel.HasMissingPrices, "complete pricing state");
}

static async Task ShellNavigation()
{
    var store = new MemoryFavouriteStore();
    var market = new FakeMarketDataService();
    var dashboard = new DashboardViewModel(store, market, [new VyrewatchMethod()]);
    var favourites = new FavouritesViewModel(store, market, TimeProvider.System);
    var money = new MoneyMakersViewModel([new VyrewatchMethod()], new MoneyMakingCalculator(), market);
    var shell = new ShellViewModel(dashboard, favourites, money);

    await shell.InitializeAsync();
    await shell.NavigateCommand.ExecuteAsync("Favourites");

    Equal(PageKind.Favourites, shell.CurrentPageKind, "selected page");
    True(ReferenceEquals(favourites, shell.CurrentPage), "active page instance");
}

static MarketDataService CreateMarketService(FakePriceClient client, DateTimeOffset? now = null) => new(
    client,
    new MarketDataOptions
    {
        LatestCacheDuration = TimeSpan.FromMinutes(5),
        MappingCacheDuration = TimeSpan.FromHours(1),
        HistoryCacheDuration = TimeSpan.FromMinutes(5),
        HistoryWindow = TimeSpan.FromDays(7)
    },
    new TestTimeProvider(now ?? new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero)));

static ItemMapping Map(int id, string name) => new(id, name, string.Empty, true, null, string.Empty);
static ItemPrice Quote(int itemId, long value) => new(itemId, value, value, null, null);
static PricePoint Point(DateTimeOffset timestamp, long value) => new(timestamp, value, value, 10, 20);
static string CreateTempDirectory()
{
    var path = Path.Combine(Path.GetTempPath(), "RunescapeTools.Tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
}

static void Equal<T>(T expected, T actual, string label) where T : notnull
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{label}: expected {expected}, actual {actual}");
}

static void EqualDecimal(decimal expected, decimal actual, string label, decimal tolerance = 0m)
{
    if (Math.Abs(expected - actual) > tolerance)
        throw new InvalidOperationException($"{label}: expected {expected}, actual {actual}");
}

static void True(bool condition, string label)
{
    if (!condition)
        throw new InvalidOperationException(label);
}

sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

sealed class FakePriceClient : IOsrsPriceClient
{
    public IReadOnlyList<ItemMapping> Mapping { get; init; } = [];
    public IReadOnlyDictionary<int, ItemPrice> Latest { get; init; } = new Dictionary<int, ItemPrice>();
    public IReadOnlyList<PricePoint> History { get; init; } = [];
    public int MappingCalls { get; private set; }
    public int LatestCalls { get; private set; }
    public int HistoryCalls { get; private set; }

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
        return Task.FromResult(History);
    }
}

sealed class MemoryFavouriteStore(params FavouriteItem[] initial) : IFavouriteStore
{
    private readonly List<FavouriteItem> items = [.. initial];

    public Task<IReadOnlyList<FavouriteItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FavouriteItem>>(items.OrderBy(item => item.Name).ToArray());

    public Task AddAsync(FavouriteItem favourite, CancellationToken cancellationToken = default)
    {
        if (items.All(item => item.ItemId != favourite.ItemId))
            items.Add(favourite);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int itemId, CancellationToken cancellationToken = default)
    {
        items.RemoveAll(item => item.ItemId == itemId);
        return Task.CompletedTask;
    }
}

sealed class FakeMarketDataService : IMarketDataService
{
    public IReadOnlyDictionary<int, ItemPrice> Latest { get; init; } = new Dictionary<int, ItemPrice>();
    public IReadOnlyList<ItemMapping> SearchResults { get; init; } = [];
    public IReadOnlyList<PricePoint> History { get; init; } = [];
    public List<int> HistoryRequests { get; } = [];
    public Exception? Failure { get; set; }

    public Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestForAsync(IEnumerable<int> itemIds, CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
            throw Failure;
        var requested = itemIds.ToHashSet();
        return Task.FromResult<IReadOnlyDictionary<int, ItemPrice>>(
            Latest.Where(pair => requested.Contains(pair.Key)).ToDictionary());
    }

    public Task<IReadOnlyList<ItemMapping>> SearchItemsAsync(string query, int take = 8, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ItemMapping>>(SearchResults.Take(take).ToArray());

    public Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(int itemId, CancellationToken cancellationToken = default)
    {
        HistoryRequests.Add(itemId);
        return Task.FromResult(History);
    }
}

sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> responses = new(responses);
    public int Calls { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(responses.Dequeue());
    }
}
