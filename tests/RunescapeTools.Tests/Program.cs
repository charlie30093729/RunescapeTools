using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using LiveChartsCore.SkiaSharpView;
using RunescapeTools.Application.Favourites;
using RunescapeTools.Application.Market;
using RunescapeTools.Application.MoneyMaking;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Application.Training;
using RunescapeTools.Core.Favourites;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;
using RunescapeTools.Core.MoneyMaking.Methods;
using RunescapeTools.Core.Profiles;
using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Configuration;
using RunescapeTools.Infrastructure.Market;
using RunescapeTools.Infrastructure.Persistence;
using RunescapeTools.Infrastructure.Profiles;
using RunescapeTools.Infrastructure.Training;
using RunescapeTools.Wpf.ViewModels;
using RunescapeTools.Wpf.Views;

var tests = new (string Name, Func<Task> Run)[]
{
    ("generic flow calculation", () => RunSync(GenericFlowCalculation)),
    ("Vyrewatch matches the legacy formula", () => RunSync(VyrewatchMatchesLegacyFormula)),
    ("Vyrewatch supports the no-regen configuration", () => RunSync(VyrewatchNoRegenConfiguration)),
    ("Vyrewatch exposes every required item once", () => RunSync(VyrewatchItemIdsAreDistinct)),
    ("mid price falls back to the available quote", () => RunSync(MidPriceFallback)),
    ("latest prices are cached and missing prices are omitted", LatestPricesAreCached),
    ("history windows are filtered and cached by resolution", HistoryWindowsAreFilteredAndCached),
    ("search favours prefix matches and respects limits", SearchOrdering),
    ("favourite warmup requests every saved history", FavouriteWarmup),
    ("Wiki client retries transient responses", WikiClientRetries),
    ("JSON store seeds, sorts, and prevents duplicates", JsonStoreSeedsSortsAndDeduplicates),
    ("JSON store never overwrites existing state", JsonStoreDoesNotOverwrite),
    ("hiscore parser maps every current OSRS skill in API order", HiscoreParserMapsSkills),
    ("profile skill icons map to official Wiki assets", ProfileSkillIconMapping),
    ("hiscore parser rejects incomplete and malformed skill rows", HiscoreParserRejectsInvalidResponses),
    ("hiscore client URL-encodes RSNs and distinguishes missing accounts", HiscoreClientProtocol),
    ("profile preference seeds bottleo and persists successful selections", ProfilePreferencePersistence),
    ("profile context preserves valid state on failure and publishes refreshes", ProfileContextStateFlow),
    ("dashboard view-model loads and reports failures", DashboardViewModelStates),
    ("favourites view-model searches, adds, selects, and removes", FavouritesViewModelFlow),
    ("favourites chart uses discrete one-day to one-month zoom", FavouritesChartZoomFlow),
    ("favourites chart points retain rolling 24-hour volume", () => RunSync(FavouritesChartVolume)),
    ("money-maker view-model shares and resets the priced selection", MoneyMakerViewModelFlow),
    ("money-maker action-rate overrides persist atomically", MoneyMakingPreferencePersistence),
    ("profile view-model loads defaults and keeps valid data on errors", ProfileViewModelFlow),
    ("EHP catalogue covers every skill and ordered rate band", () => RunSync(EhpCatalogueCoverage)),
    ("catalogue market resources keep valid local item identities", () => RunSync(CatalogueMarketItemIntegrity)),
    ("training definitions support stable default and alternative methods", () => RunSync(TrainingMethodSelection)),
    ("XP Planner rows select and persist training methods", () => RunSync(XpPlannerRowMethodSelection)),
    ("skill configuration defaults and calculation effects are applied centrally", () => RunSync(TrainingSkillConfiguration)),
    ("XP Planner rows persist and reset skill configuration", () => RunSync(XpPlannerRowConfiguration)),
    ("approved deterministic methods expose reviewed rates and economics", () => RunSync(DeterministicMethodCatalogue)),
    ("Herblore methods use shared equipment and four-dose economics", () => RunSync(HerbloreEquipmentEconomics)),
    ("Herblore alternatives preserve unlock routes and reviewed rates", () => RunSync(HerbloreAlternativeMethods)),
    ("practical buyable alternatives expose reviewed unlocks, rates, and economics", () => RunSync(PracticalBuyableMethods)),
    ("Runecraft alternatives and Raiments configuration preserve reviewed mechanics", () => RunSync(RunecraftAlternativeMethods)),
    ("phase-two methods expose reviewed unlocks, rates, and item flows", () => RunSync(PhaseTwoMethodCatalogue)),
    ("phase-two calculations reproduce reviewed resource totals and pricing", () => RunSync(PhaseTwoTrainingCalculations)),
    ("phase-three methods expose reviewed rates and Sailing item flows", () => RunSync(PhaseThreeMethodCatalogue)),
    ("phase-three calculations reproduce reviewed hours and Sailing resources", () => RunSync(PhaseThreeTrainingCalculations)),
    ("Farming tree runs price saplings, protection, and clearing fees", () => RunSync(FarmingTrainingCalculations)),
    ("combat methods expose reviewed rates, zero-time flags, and supplies", () => RunSync(CombatMethodCatalogue)),
    ("Slayer credit reduces zero-time Magic cost without changing profile XP", () => RunSync(CombatDependencyCalculations)),
    ("Construction route reproduces Main EHP hours and live-price economics", () => RunSync(ConstructionTrainingCalculation)),
    ("training rate overrides scale hours without changing total resources", () => RunSync(TrainingRateOverride)),
    ("hourly training costs respond to personal rate overrides", () => RunSync(HourlyTrainingEconomics)),
    ("money-maker profit applies only to selected non-negative skill hours", () => RunSync(TrainingMoneyMakerAllocation)),
    ("training plans persist independently per RSN", TrainingPlanPersistence),
    ("XP Planner tooltips use live high buys and low sells", () => RunSync(XpPlannerPriceTooltips)),
    ("XP planner allocates money-maker profit to selected skill hours", XpPlannerViewModelFlow),
    ("XP planner remains usable when live prices fail", XpPlannerPriceFailure),
    ("shell navigation loads the requested page", ShellNavigation),
    ("WPF profile, Favourites, Money Makers, and XP Planner views construct successfully", WpfViewsConstruct)
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

    var fourAccounts = new MoneyMakingCalculator().Calculate(method, prices, 4);
    Equal(4, fourAccounts.Method.Accounts, "manual account quantity");
    EqualDecimal(18_400m, fourAccounts.ProfitAllAccounts, "manual account total profit");
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

static void VyrewatchNoRegenConfiguration()
{
    var method = VyrewatchMethod.CreateDefinition(usingRegenPotions: false);
    var prices = new VyrewatchMethod().Definition.RequiredItemIds
        .ToDictionary(id => id, id => Quote(id, 1_000));
    var result = new MoneyMakingCalculator().Calculate(method, prices);

    EqualDecimal(88m, method.ActionsPerHour, "no-regen kills per hour");
    True(
        method.Items.All(item => item.ItemId != 30125),
        "no-regen configuration removes prayer regeneration potions");
    EqualDecimal(2_000m, result.InputCostPerAccount, "no-regen hourly supplies");
    True(
        result.Lines.All(line => line.Item.ItemId != 30125),
        "no-regen ledger excludes prayer regeneration potions");
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

static async Task HistoryWindowsAreFilteredAndCached()
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

    Equal(2, weeklyFirst.Count, "filtered weekly history count");
    Equal(weeklyFirst.Count, weeklySecond.Count, "cached weekly history count");
    Equal(2, monthlyFirst.Count, "filtered monthly history count");
    Equal(monthlyFirst.Count, monthlySecond.Count, "cached monthly history count");
    Equal(2, client.HistoryCalls, "one API call per history resolution");
    True(
        client.HistoryTimeSteps.SequenceEqual(
            [PriceTimeStep.OneHour, PriceTimeStep.SixHours]),
        "history resolutions are cached independently");
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

static Task HiscoreParserMapsSkills()
{
    var now = new DateTimeOffset(2026, 7, 18, 10, 30, 0, TimeSpan.Zero);
    var parser = new HiscoreParser(new TestTimeProvider(now));

    var profile = parser.Parse("  bottleo  ", HiscoreResponse());

    Equal("bottleo", profile.Rsn, "trimmed RSN");
    Equal(123, profile.OverallRank, "overall rank");
    Equal(2_376, profile.TotalLevel, "total level");
    Equal(4_567_890_123L, profile.TotalExperience, "long total experience");
    Equal(24, profile.Skills.Count, "current skill count");
    Equal("Attack", profile.Skills[0].Name, "first skill");
    Equal("Hitpoints", profile.Skills[3].Name, "API constitution alias");
    Equal("Runecraft", profile.Skills[20].Name, "API runecrafting alias");
    Equal("Sailing", profile.Skills[^1].Name, "latest skill");
    Equal(now, profile.RetrievedAtUtc, "retrieval time");
    return Task.CompletedTask;
}

static Task ProfileSkillIconMapping()
{
    foreach (var skill in OsrsHiscoreSkillOrder.Skills)
    {
        Equal(
            $"https://oldschool.runescape.wiki/images/{skill}_icon.png",
            OsrsSkillIconMap.GetIconUrl(skill) ?? string.Empty,
            $"{skill} icon URL");
    }

    Equal(
        "https://oldschool.runescape.wiki/images/Runecraft_icon.png",
        OsrsSkillIconMap.GetIconUrl("Runecraft") ?? string.Empty,
        "Runecraft uses the documented asset name");
    True(OsrsSkillIconMap.GetIconUrl("Runecrafting") is null, "Runecrafting is not a valid display mapping");
    True(OsrsSkillIconMap.GetIconUrl("Unexpected skill") is null, "unknown skills use the UI fallback");
    return Task.CompletedTask;
}

static async Task HiscoreParserRejectsInvalidResponses()
{
    var parser = new HiscoreParser(TimeProvider.System);
    await ThrowsAsync<HiscoreParseException>(
        () => Task.FromResult(parser.Parse("bottleo", string.Join('\n', HiscoreResponse().Split('\n').Take(24)))),
        "incomplete response");

    var rows = HiscoreResponse().Split('\n');
    rows[5] = "not-a-rank,99,13034431";
    await ThrowsAsync<HiscoreParseException>(
        () => Task.FromResult(parser.Parse("bottleo", string.Join('\n', rows))),
        "malformed response");
}

static async Task HiscoreClientProtocol()
{
    var successHandler = new SequenceHandler(new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(HiscoreResponse(), Encoding.UTF8, "text/plain")
    });
    using var successHttp = new HttpClient(successHandler)
    {
        BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/")
    };
    var client = new OsrsHiscoreClient(successHttp);

    await client.GetRawHiscoresAsync("  Name With Space  ");

    True(
        successHandler.LastRequestUri?.AbsoluteUri.EndsWith("index_lite.ws?player=Name%20With%20Space", StringComparison.Ordinal) == true,
        "URL-encoded standard endpoint");

    var missingHandler = new SequenceHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
    using var missingHttp = new HttpClient(missingHandler) { BaseAddress = successHttp.BaseAddress };
    await ThrowsAsync<PlayerNotFoundException>(
        () => new OsrsHiscoreClient(missingHttp).GetRawHiscoresAsync("Missing Player"),
        "not-found response");
}

static async Task ProfilePreferencePersistence()
{
    var directory = CreateTempDirectory();
    try
    {
        var path = Path.Combine(directory, "profile.json");
        var store = new JsonProfilePreferenceStore(new ProfilePreferenceOptions
        {
            FilePath = path,
            DefaultRsn = "bottleo"
        });

        Equal("bottleo", await store.GetSelectedRsnAsync(), "first-run default");
        True(File.Exists(path), "profile preference file exists");

        await store.SetSelectedRsnAsync("  Zezima  ");
        var reopened = new JsonProfilePreferenceStore(new ProfilePreferenceOptions
        {
            FilePath = path,
            DefaultRsn = "bottleo"
        });
        Equal("Zezima", await reopened.GetSelectedRsnAsync(), "persisted selected RSN");
        True(!File.Exists(path + ".tmp"), "atomic profile temporary file replaced");
    }
    finally
    {
        Directory.Delete(directory, true);
    }
}

static async Task ProfileContextStateFlow()
{
    var client = new FakeHiscoreClient();
    var preference = new MemoryProfilePreferenceStore("bottleo");
    var context = new CurrentProfileContext(
        client,
        new HiscoreParser(new TestTimeProvider(new DateTimeOffset(2026, 7, 18, 0, 0, 0, TimeSpan.Zero))),
        preference);
    var changes = 0;
    context.ProfileChanged += (_, _) => changes++;

    await context.LoadSelectedProfileAsync();
    Equal("bottleo", context.CurrentRsn ?? string.Empty, "loaded saved profile");
    Equal(1, changes, "initial notification");

    client.Handler = (rsn, _) => throw new PlayerNotFoundException(rsn);
    await ThrowsAsync<PlayerNotFoundException>(
        () => context.LoadProfileAsync("missing"),
        "failed selection");
    Equal("bottleo", context.CurrentRsn ?? string.Empty, "valid profile retained");
    Equal("bottleo", preference.SelectedRsn, "failed RSN not persisted");
    Equal(1, changes, "failed load does not notify");

    client.Handler = async (_, cancellationToken) =>
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        return HiscoreResponse();
    };
    using (var cancellation = new CancellationTokenSource())
    {
        cancellation.Cancel();
        await ThrowsAsync<OperationCanceledException>(
            () => context.LoadProfileAsync("cancelled", cancellation.Token),
            "cancelled profile request");
    }
    Equal("bottleo", context.CurrentRsn ?? string.Empty, "cancellation retains profile");
    Equal(1, changes, "cancellation does not notify");

    client.Handler = (_, _) => Task.FromResult(HiscoreResponse(98));
    await context.RefreshAsync();
    Equal(2, changes, "refresh notification");
    Equal(98, context.CurrentProfile?.Skills[0].Level ?? 0, "refreshed profile data");
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

static async Task FavouritesChartZoomFlow()
{
    var now = new DateTimeOffset(2026, 7, 28, 12, 0, 0, TimeSpan.Zero);
    var store = new MemoryFavouriteStore(
        new FavouriteItem(1, "Rune bar", now));
    var market = new FakeMarketDataService
    {
        Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 500) },
        HistoryByTimeStep = new Dictionary<PriceTimeStep, IReadOnlyList<PricePoint>>
        {
            [PriceTimeStep.OneHour] =
            [
                Point(now.AddDays(-6), 400),
                Point(now.AddDays(-3), 450),
                Point(now.AddHours(-12), 500)
            ],
            [PriceTimeStep.SixHours] =
            [
                Point(now.AddDays(-29), 300),
                Point(now.AddDays(-14), 400),
                Point(now.AddHours(-6), 500)
            ]
        }
    };
    var viewModel = new FavouritesViewModel(
        store,
        market,
        new TestTimeProvider(now));

    await viewModel.LoadAsync();

    Equal("7 DAYS", viewModel.HistoryWindowLabel, "default history window");
    var series = (LineSeries<FavouritePriceChartPoint>)viewModel.ChartSeries.Single();
    Equal(3, series.Values?.Count() ?? 0, "chart uses enriched price and volume points");
    viewModel.ZoomHistoryCommand.Execute(120);
    Equal("3 DAYS", viewModel.HistoryWindowLabel, "first zoom-in window");
    viewModel.ZoomHistoryCommand.Execute(120);
    Equal("1 DAY", viewModel.HistoryWindowLabel, "second zoom-in window");
    viewModel.ZoomHistoryCommand.Execute(120);
    Equal("1 DAY", viewModel.HistoryWindowLabel, "minimum history window");

    viewModel.ZoomHistoryCommand.Execute(-120);
    Equal("3 DAYS", viewModel.HistoryWindowLabel, "first zoom-out window");
    viewModel.ZoomHistoryCommand.Execute(-120);
    Equal("7 DAYS", viewModel.HistoryWindowLabel, "default zoom-out window");
    viewModel.ZoomHistoryCommand.Execute(-120);
    Equal("1 MONTH", viewModel.HistoryWindowLabel, "maximum history window");
    viewModel.ZoomHistoryCommand.Execute(-120);
    Equal("1 MONTH", viewModel.HistoryWindowLabel, "clamped maximum history window");
    True(
        market.HistoryTimeSteps.SequenceEqual(
            [PriceTimeStep.SixHours, PriceTimeStep.OneHour]),
        "favourites loads hourly and monthly resolutions");
    True(
        market.HistoryWindows.SequenceEqual(
            [TimeSpan.FromDays(31), TimeSpan.FromDays(8)]),
        "favourites loads a one-day rolling-volume lookback");
}

static void FavouritesChartVolume()
{
    var now = new DateTimeOffset(2026, 8, 2, 12, 0, 0, TimeSpan.Zero);
    IReadOnlyList<PricePoint> history =
    [
        new PricePoint(now.AddHours(-25), 450, 450, 100, 200),
        new PricePoint(now.AddHours(-24), 460, 460, 10, 20),
        new PricePoint(now.AddHours(-23), 470, 470, 1, 2),
        new PricePoint(now, 500, 500, 3, 4)
    ];

    var points = FavouritesViewModel.BuildChartPoints(
        history,
        now.AddDays(-1),
        now);
    var current = points.Single(point => point.Timestamp == now);

    Equal(4L, current.RollingHighVolume, "rolling high-side volume");
    Equal(6L, current.RollingLowVolume, "rolling low-side volume");
    Equal(10L, current.RollingVolume, "rolling total volume");
    True(current.TooltipText.Contains("24h tracked volume: 10 items"), "volume tooltip total");
    True(current.TooltipText.Contains("High side: 4"), "volume tooltip high side");
    True(current.TooltipText.Contains("Low side: 6"), "volume tooltip low side");
}

static async Task MoneyMakerViewModelFlow()
{
    var method = new VyrewatchMethod();
    var secondMethod = new ZulrahMethod();
    var selection = new MoneyMakerSelectionContext();
    var preferences = new MemoryMoneyMakingPreferenceStore();
    var market = new FakeMarketDataService
    {
        Latest = method.Definition.RequiredItemIds
            .Concat(secondMethod.Definition.RequiredItemIds)
            .Distinct()
            .ToDictionary(id => id, id => Quote(id, 1_000))
    };
    var viewModel = new MoneyMakersViewModel(
        [method, secondMethod],
        new MoneyMakingCalculator(),
        market,
        preferences,
        selection);

    await viewModel.LoadAsync();
    True(viewModel.SelectedMethod is null, "money maker should require an explicit selection");
    var primaryRow = viewModel.Methods.Single(row => row.Method.Definition.Slug == method.Definition.Slug);
    var secondaryRow = viewModel.Methods.Single(row => row.Method.Definition.Slug == secondMethod.Definition.Slug);
    viewModel.SelectedMethod = primaryRow;

    Equal(method.Definition.Items.Count, viewModel.FlowRows.Count, "money-making ledger rows");
    True(viewModel.ProfitAllAccounts.EndsWith(" gp", StringComparison.Ordinal), "formatted total profit");
    True(!viewModel.HasMissingPrices, "complete pricing state");
    Equal(method.Definition.Slug, selection.Current?.Slug ?? string.Empty, "shared money-maker selection");
    Equal(method.Definition.Accounts, viewModel.AccountCount, "method default account quantity");
    Equal(method.Definition.Accounts, selection.Current?.AccountCount ?? 0, "shared default account quantity");
    True(viewModel.ShowRegenPotionOption, "Vyrewatch exposes the regen-potion option");
    True(viewModel.UsingRegenPotions, "Vyrewatch defaults to the current regen configuration");
    EqualDecimal(102m, viewModel.ActionsPerHour, "Vyrewatch regen default action rate");
    True(!viewModel.IsActionsPerHourOverridden, "default action rate is not an override");

    viewModel.ActionsPerHour = 95m;
    EqualDecimal(95m, viewModel.ActionsPerHour, "custom action rate");
    EqualDecimal(
        95m,
        preferences.Overrides[method.Definition.Slug],
        "custom action rate persisted by method slug");
    var customResult = new MoneyMakingCalculator().Calculate(
        method.Definition with { ActionsPerHour = 95m },
        market.Latest,
        method.Definition.Accounts);
    EqualDecimal(
        customResult.ProfitPerAccount,
        selection.Current?.ProfitPerAccountPerHour ?? 0m,
        "custom action rate updates the shared XP Planner profit rate");
    True(viewModel.IsActionsPerHourOverridden, "custom action rate indicator");
    True(
        viewModel.MethodKicker.StartsWith("95 actions / hour", StringComparison.Ordinal),
        "custom action rate reprices the method");

    viewModel.UsingRegenPotions = false;
    Equal(9, viewModel.FlowRows.Count, "no-regen ledger removes the prayer regeneration potion");
    True(
        viewModel.FlowRows.All(row => row.Name != "Prayer regeneration potion(4)"),
        "no-regen ledger contains no prayer regeneration potion row");
    True(
        viewModel.MethodKicker.StartsWith("95 actions / hour", StringComparison.Ordinal),
        "custom action rate survives a Vyrewatch configuration change");
    viewModel.ResetActionsPerHourCommand.Execute(null);
    EqualDecimal(88m, viewModel.ActionsPerHour, "no-regen reset action rate");
    True(
        !preferences.Overrides.ContainsKey(method.Definition.Slug),
        "reset removes the persisted override");
    True(!viewModel.IsActionsPerHourOverridden, "reset clears custom action indicator");

    var profitPerAccount = selection.Current?.ProfitPerAccountPerHour ?? 0m;
    viewModel.IncreaseAccountCountCommand.Execute(null);
    Equal(method.Definition.Accounts + 1, viewModel.AccountCount, "account quantity increments");
    Equal(method.Definition.Accounts + 1, selection.Current?.AccountCount ?? 0, "shared account quantity increments");
    True(
        viewModel.MethodKicker.StartsWith("88 actions / hour", StringComparison.Ordinal),
        "account changes preserve the no-regen configuration");
    EqualDecimal(
        profitPerAccount * (method.Definition.Accounts + 1),
        selection.Current?.TotalProfitPerHour ?? 0m,
        "shared all-account profit");
    viewModel.SelectedMethod = secondaryRow;
    Equal(secondMethod.Definition.Accounts, viewModel.AccountCount, "second method uses its own account quantity");
    EqualDecimal(31m, viewModel.ActionsPerHour, "second method uses its own default action rate");
    viewModel.ActionsPerHour = 25m;
    EqualDecimal(
        25m,
        preferences.Overrides[secondMethod.Definition.Slug],
        "generic method action rate persisted");
    viewModel.SelectedMethod = primaryRow;
    Equal(method.Definition.Accounts + 1, viewModel.AccountCount, "account quantity is retained per method");
    EqualDecimal(88m, viewModel.ActionsPerHour, "Vyrewatch retains its current default independently");
    viewModel.SelectedMethod = secondaryRow;
    EqualDecimal(25m, viewModel.ActionsPerHour, "generic method override retained across selection changes");
    viewModel.SelectedMethod = primaryRow;
    viewModel.DecreaseAccountCountCommand.Execute(null);
    Equal(method.Definition.Accounts, viewModel.AccountCount, "account quantity decrements");

    selection.Clear();
    True(viewModel.SelectedMethod is null, "external reset clears Money Makers selection");
    Equal(0, viewModel.FlowRows.Count, "external reset clears displayed ledger");

    var restoredViewModel = new MoneyMakersViewModel(
        [method, secondMethod],
        new MoneyMakingCalculator(),
        market,
        preferences,
        new MoneyMakerSelectionContext());
    await restoredViewModel.LoadAsync();
    restoredViewModel.SelectedMethod = restoredViewModel.Methods.Single(
        row => row.Method.Definition.Slug == secondMethod.Definition.Slug);
    EqualDecimal(25m, restoredViewModel.ActionsPerHour, "saved generic override restores after restart");
}

static async Task MoneyMakingPreferencePersistence()
{
    var directory = CreateTempDirectory();
    try
    {
        var path = Path.Combine(directory, "money-making-preferences.json");
        var store = new JsonMoneyMakingPreferenceStore(
            new MoneyMakingPreferenceOptions { FilePath = path });

        await store.SetActionsPerHourOverrideAsync("Vyrewatch-Sentinels", 95m);
        await store.SetActionsPerHourOverrideAsync("zulrah", 27.5m);

        var saved = await store.GetActionsPerHourOverridesAsync();
        EqualDecimal(95m, saved["vyrewatch-sentinels"], "persisted Vyrewatch override");
        EqualDecimal(27.5m, saved["ZULRAH"], "case-insensitive persisted override");

        await store.SetActionsPerHourOverrideAsync("zulrah", null);
        var afterReset = await store.GetActionsPerHourOverridesAsync();
        True(!afterReset.ContainsKey("zulrah"), "reset removes persisted override");
        True(!File.Exists(path + ".tmp"), "atomic preference replacement leaves no temporary file");
        await ThrowsAsync<ArgumentOutOfRangeException>(
            () => store.SetActionsPerHourOverrideAsync("vyrewatch-sentinels", 0m),
            "non-positive action rate");
    }
    finally
    {
        Directory.Delete(directory, true);
    }
}

static async Task ProfileViewModelFlow()
{
    var client = new FakeHiscoreClient();
    var preference = new MemoryProfilePreferenceStore("bottleo");
    var context = new CurrentProfileContext(client, new HiscoreParser(TimeProvider.System), preference);
    var viewModel = new ProfileViewModel(context);

    await viewModel.LoadAsync();
    Equal("bottleo", viewModel.ProfileRsn, "default profile RSN");
    Equal(24, viewModel.Skills.Count, "displayed skill count");

    client.Handler = (rsn, _) => throw new PlayerNotFoundException(rsn);
    viewModel.SearchRsn = "does not exist";
    await viewModel.SearchCommand.ExecuteAsync(null);
    Equal("bottleo", viewModel.ProfileRsn, "failed search retains profile");
    True(!string.IsNullOrWhiteSpace(viewModel.ErrorMessage), "failed search error");

    client.Handler = (_, _) => Task.FromResult(HiscoreResponse(75));
    viewModel.SearchRsn = "  New Player  ";
    await viewModel.SearchCommand.ExecuteAsync(null);
    Equal("New Player", viewModel.ProfileRsn, "successful searched profile");
    Equal("New Player", preference.SelectedRsn, "successful search persisted");
    Equal("75", viewModel.Skills[0].Level, "updated skill level");
}

static void EhpCatalogueCoverage()
{
    var catalogue = new MainEhpCatalogue();
    var expectedSkills = OsrsHiscoreSkillOrder.Skills
        .Where(skill => skill is not "Attack" and not "Strength" and not "Hitpoints")
        .ToArray();
    Equal(21, catalogue.Skills.Count, "catalogue skill count");
    Equal(
        string.Join('|', expectedSkills),
        string.Join('|', catalogue.Skills.Select(skill => skill.Skill)),
        "catalogue skill order");

    foreach (var skill in catalogue.Skills)
    {
        True(skill.Bands.Count > 0, $"{skill.Skill} should have at least one rate band");
        True(skill.Bands.All(band => band.ExperiencePerHour > 0), $"{skill.Skill} rates should be positive");
        var ordered = skill.Bands.OrderBy(band => band.StartExperience).ToArray();
        Equal(ordered.Length, ordered.Select(band => band.StartExperience).Distinct().Count(), $"{skill.Skill} band starts");
        Equal(string.Join('|', ordered.Select(band => band.StartExperience)), string.Join('|', skill.Bands.Select(band => band.StartExperience)), $"{skill.Skill} band ordering");
        var expectedMethodCount = skill.Skill switch
        {
            "Herblore" => 3,
            "Smithing" => 3,
            "Runecraft" => 3,
            "Farming" => 2,
            "Prayer" or "Fletching" or "Crafting" or "Construction" => 2,
            _ => 1
        };
        Equal(expectedMethodCount, skill.AvailableMethods.Count, $"{skill.Skill} method count");
        Equal(skill.DefaultMethodId, skill.AvailableMethods[0].Id, $"{skill.Skill} default method ID");
        Equal(skill.Bands.Count, skill.AvailableMethods[0].Bands.Count, $"{skill.Skill} default method bands");
    }
}

static void CatalogueMarketItemIntegrity()
{
    var resources = new MainEhpCatalogue().Skills
        .SelectMany(skill => skill.AvailableMethods)
        .SelectMany(method => method.Bands)
        .SelectMany(band => band.Economics?.Resources ?? [])
        .ToArray();

    True(resources.Length > 0, "catalogue should expose market resources");
    True(resources.All(resource => resource.ItemId > 0), "catalogue item IDs should be positive");
    True(
        resources.All(resource => !string.IsNullOrWhiteSpace(resource.Name)),
        "catalogue items should have display names");

    var idConflicts = resources
        .GroupBy(resource => resource.ItemId)
        .Select(group => new
        {
            ItemId = group.Key,
            Names = group
                .Select(resource => BaseItemName(resource.Name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order()
                .ToArray()
        })
        .Where(group => group.Names.Length > 1)
        .ToArray();
    Equal(
        string.Empty,
        string.Join("; ", idConflicts.Select(group => $"{group.ItemId}: {string.Join('|', group.Names)}")),
        "one item ID should not map to conflicting base names");

    var nameConflicts = resources
        .GroupBy(resource => BaseItemName(resource.Name), StringComparer.OrdinalIgnoreCase)
        .Select(group => new
        {
            Name = group.Key,
            ItemIds = group.Select(resource => resource.ItemId).Distinct().Order().ToArray()
        })
        .Where(group => group.ItemIds.Length > 1)
        .ToArray();
    Equal(
        string.Empty,
        string.Join("; ", nameConflicts.Select(group => $"{group.Name}: {string.Join('|', group.ItemIds)}")),
        "one base item name should not map to conflicting IDs");
}

static string BaseItemName(string name)
{
    var contextStart = name.IndexOf(" (", StringComparison.Ordinal);
    return contextStart < 0 ? name : name[..contextStart];
}

static void TrainingMethodSelection()
{
    var main = new TrainingMethodDefinition(
        "main",
        "Main route",
        [new TrainingRateBand(0, 100m, "Main route")]);
    var alternative = new TrainingMethodDefinition(
        "alternative",
        "Alternative route",
        [new TrainingRateBand(0, 200m, "Alternative route")]);
    var definition = new TrainingSkillDefinition(
        "Method test",
        main.Bands,
        Methods: [main, alternative],
        DefaultMethodId: main.Id);
    var calculator = new TrainingPlanCalculator();

    var defaultResult = calculator.Calculate(definition, 0, 1_000, new Dictionary<int, ItemPrice>());
    var alternativeResult = calculator.Calculate(
        definition,
        0,
        1_000,
        new Dictionary<int, ItemPrice>(),
        methodId: alternative.Id);

    Equal("main", defaultResult.Method.Id, "resolved default method");
    EqualDecimal(10m, defaultResult.Hours, "default method hours");
    Equal("alternative", alternativeResult.Method.Id, "resolved alternative method");
    EqualDecimal(5m, alternativeResult.Hours, "alternative method hours");
}

static void XpPlannerRowMethodSelection()
{
    var main = new TrainingMethodDefinition(
        "main",
        "Main route",
        [new TrainingRateBand(0, 100m, "Main training method")]);
    var alternative = new TrainingMethodDefinition(
        "alternative",
        "Alternative route",
        [new TrainingRateBand(0, 200m, "Alternative training method")]);
    var definition = new TrainingSkillDefinition(
        "Method test",
        main.Bands,
        Methods: [main, alternative],
        DefaultMethodId: main.Id);
    var changes = 0;
    var row = new XpPlannerRowViewModel(
        definition,
        new TrainingPlanCalculator(),
        0,
        null,
        new Dictionary<int, ItemPrice>(),
        () => changes++);

    Equal("main", row.SelectedMethodOption?.Id ?? string.Empty, "row default method");
    Equal("Main training method", row.SelectedMethodOption?.Name ?? string.Empty, "active method label");
    EqualDecimal(100m, row.PersonalRate, "default method rate");

    row.SelectedMethodOption = row.MethodOptions.Single(option => option.Id == "alternative");

    Equal("alternative", row.Result.Method.Id, "selected calculation method");
    Equal("Alternative training method", row.Method, "selected active band label");
    EqualDecimal(200m, row.PersonalRate, "selected method resets to its catalogue rate");
    EqualDecimal(1m, changes, "method selection change notification");
    Equal("alternative", row.ToPreference().TrainingMethodId ?? string.Empty, "selected method preference");

    row.StartExperience = 1_000;
    row.TargetExperience = 10_000;
    row.PersonalRate = 321m;
    row.IsMoneyMakingSelected = true;
    row.ResetSkillCommand.Execute(null);

    Equal(0L, row.StartExperience, "skill reset restores profile XP");
    Equal(TrainingPlanCalculator.MaximumExperience, row.TargetExperience, "skill reset restores 200m goal");
    EqualDecimal(200m, row.PersonalRate, "skill reset uses selected method catalogue rate");
    True(!row.IsMoneyMakingSelected, "skill reset clears money-making allocation");
    Equal("alternative", row.SelectedMethodOption?.Id ?? string.Empty, "skill reset preserves selected method");

    var restoredRow = new XpPlannerRowViewModel(
        definition,
        new TrainingPlanCalculator(),
        0,
        new TrainingSkillPreference(
            definition.Skill,
            TrainingPlanCalculator.MaximumExperience,
            TrainingMethodId: alternative.Id),
        new Dictionary<int, ItemPrice>(),
        () => { });
    Equal(
        "alternative",
        restoredRow.SelectedMethodOption?.Id ?? string.Empty,
        "saved method selection restores");
}

static void ConstructionTrainingCalculation()
{
    var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Construction");
    var prices = new Dictionary<int, ItemPrice>
    {
        [8778] = new ItemPrice(8778, 431, 425, null, null),
        [8782] = new ItemPrice(8782, 1_910, 1_857, null, null)
    };
    var result = new TrainingPlanCalculator().Calculate(definition, 0, 200_000_000, prices);

    EqualDecimal(142.7895m, result.Hours, "Construction EHP hours", 0.0001m);
    Equal(199_981_753L, result.PricedExperience, "priced Construction XP");
    True(!result.IsFullyPriced, "low-level furniture should remain visibly unpriced");
    True(result.NetGp is < -2_800_000_000m and > -2_805_000_000m, "Construction cost should match the reviewed 2.8b estimate");
}

static void TrainingSkillConfiguration()
{
    var catalogue = new MainEhpCatalogue();
    var calculator = new TrainingPlanCalculator();
    var prices = new Dictionary<int, ItemPrice>();

    var prayer = catalogue.Skills.Single(skill => skill.Skill == "Prayer");
    var gilded = calculator.Calculate(
        prayer,
        0,
        1_000_000,
        prices);
    Equal(
        "Superior dragon bones at the Gilded Altar",
        gilded.Method.Bands[0].Method,
        "Prayer defaults to Gilded Altar");
    EqualDecimal(
        1m / 525m,
        Resource(gilded.Method.Bands[0], 22124).QuantityPerExperience,
        "Gilded Altar superior bone consumption");
    var prayerRow = new XpPlannerRowViewModel(
        prayer,
        calculator,
        0,
        null,
        prices,
        () => { });
    Equal(
        "Superior dragon bones",
        prayerRow.SelectedMethodOption?.Name ?? string.Empty,
        "Prayer material selector remains separate from offering location");

    var chaos = calculator.Calculate(
        prayer,
        0,
        1_000_000,
        prices,
        configuration: new Dictionary<string, string>
        {
            ["offering-location"] = "chaos-altar"
        });
    Equal(
        "Superior dragon bones at the Chaos Altar",
        chaos.Method.Bands[0].Method,
        "Prayer Chaos Altar selection");
    EqualDecimal(
        1m / 1_050m,
        Resource(chaos.Method.Bands[0], 22124).QuantityPerExperience,
        "Chaos Altar superior bone consumption");

    var firemaking = catalogue.Skills.Single(skill => skill.Skill == "Firemaking");
    var pyromancer = calculator.Calculate(
        firemaking,
        13_034_431,
        13_034_431 + 639_292,
        prices);
    EqualDecimal(623_700m * 1.025m, pyromancer.BaseRate, "Pyromancer default rate");
    EqualDecimal(
        1m / (420m * 1.025m),
        Resource(pyromancer.Method.Bands[^1], 32910).QuantityPerExperience,
        "Pyromancer rosewood consumption");

    var bonfire = calculator.Calculate(
        firemaking,
        13_034_431,
        13_034_431 + 267_832,
        prices,
        configuration: new Dictionary<string, string>
        {
            ["pyromancer-outfit"] = bool.TrueString,
            ["bonfire"] = bool.TrueString
        });
    EqualDecimal(268m * 975m * 1.025m, bonfire.BaseRate, "manual bonfire rate");
    Equal(
        "Rosewood logs - bonfire",
        bonfire.Method.Bands[^1].Method,
        "bonfire method label");

    var fletching = catalogue.Skills.Single(skill => skill.Skill == "Fletching");
    var counted = calculator.Calculate(fletching, 5_346_332, 6_346_332, prices);
    var zeroTime = calculator.Calculate(
        fletching,
        5_346_332,
        6_346_332,
        prices,
        configuration: new Dictionary<string, string>
        {
            ["include-hours"] = bool.FalseString
        });
    EqualDecimal(1m, counted.Hours, "Fletching default active hours");
    EqualDecimal(0m, zeroTime.Hours, "Fletching hidden active hours");
    True(!zeroTime.IncludesActiveHours, "Fletching zero-time flag");

    var herblore = catalogue.Skills.Single(skill => skill.Skill == "Herblore");
    var noEquipment = calculator.Calculate(
        herblore,
        2_192_818,
        2_642_818,
        prices,
        configuration: new Dictionary<string, string>
        {
            ["prescription-goggles"] = bool.FalseString,
            ["alchemists-amulet"] = bool.FalseString
        });
    var brewBand = noEquipment.Method.Bands[^1];
    EqualDecimal(
        1m / 180m,
        Resource(brewBand, 6693).QuantityPerExperience,
        "Herblore without goggles");
    True(
        brewBand.Economics!.Resources.All(resource => resource.ItemId != 21163),
        "Herblore without amulet has no charge input");
    EqualDecimal(
        3m / 4m / 180m,
        Resource(brewBand, 6685).QuantityPerExperience,
        "Herblore without amulet has base output doses");

    var construction = catalogue.Skills.Single(skill => skill.Skill == "Construction");
    var carpenter = calculator.Calculate(
        construction,
        13_034_431,
        14_474_431,
        prices,
        configuration: new Dictionary<string, string>
        {
            ["carpenters-outfit"] = bool.TrueString
        });
    EqualDecimal(1_440_000m * 1.025m, carpenter.BaseRate, "Carpenter outfit rate");
    EqualDecimal(
        1m / 140m / 1.025m,
        Resource(carpenter.Method.Bands[^1], 8782).QuantityPerExperience,
        "Carpenter outfit plank consumption");

    var configuredSkills = catalogue.Skills
        .Where(skill => skill.Configurator is not null)
        .Select(skill => skill.Skill)
        .ToArray();
    Equal(
        "Prayer|Fletching|Firemaking|Smithing|Herblore|Farming|Runecraft|Construction",
        string.Join('|', configuredSkills),
        "requested skill configurators");
    Equal(
        0,
        catalogue.Skills.Single(skill => skill.Skill == "Farming")
            .Configurator!.Definition.Options.Count,
        "Farming placeholder configuration");
}

static void XpPlannerRowConfiguration()
{
    var definition = new MainEhpCatalogue().Skills
        .Single(skill => skill.Skill == "Fletching");
    var row = new XpPlannerRowViewModel(
        definition,
        new TrainingPlanCalculator(),
        5_346_332,
        null,
        new Dictionary<int, ItemPrice>(),
        () => { });

    True(row.HasConfiguration, "Fletching row exposes configuration");
    row.ApplyConfiguration(new Dictionary<string, string>
    {
        ["include-hours"] = bool.FalseString
    });
    EqualDecimal(0m, row.Result.Hours, "row applies Fletching zero-time selection");
    Equal(
        bool.FalseString,
        row.ToPreference().Configuration!["include-hours"],
        "row persists Fletching configuration");

    row.ResetSkillCommand.Execute(null);
    Equal(
        bool.TrueString,
        row.ConfigurationValues["include-hours"],
        "skill reset restores configuration default");
    True(row.Result.IncludesActiveHours, "skill reset restores active hours");

    var restored = new XpPlannerRowViewModel(
        definition,
        new TrainingPlanCalculator(),
        5_346_332,
        new TrainingSkillPreference(
            "Fletching",
            TrainingPlanCalculator.MaximumExperience,
            Configuration: new Dictionary<string, string>
            {
                ["include-hours"] = bool.FalseString
            }),
        new Dictionary<int, ItemPrice>(),
        () => { });
    True(!restored.Result.IncludesActiveHours, "saved configuration restores");
}

static void DeterministicMethodCatalogue()
{
    var catalogue = new MainEhpCatalogue();

    var prayer = TrainingBand(catalogue, "Prayer", 0);
    EqualDecimal(2_000_000m, prayer.ExperiencePerHour, "Prayer rate");
    Equal("Superior dragon bones at the Gilded Altar", prayer.Method, "Prayer method");
    EqualDecimal(1m / 525m, Resource(prayer, 22124).QuantityPerExperience, "Prayer bones per XP");

    var cooking = TrainingBand(catalogue, "Cooking", 8_771_558);
    EqualDecimal(490_000m, cooking.ExperiencePerHour, "Cooking rate");
    Equal("Bake Pie spell - summer pies", cooking.Method, "Cooking method");
    Equal(
        "7216|7218|9075",
        string.Join('|', cooking.Economics!.Resources.Select(resource => resource.ItemId).Order()),
        "Cooking item IDs");
    EqualDecimal(1m / 260m, Resource(cooking, 7216).QuantityPerExperience, "raw summer pies per XP");

    var crafting = TrainingBand(catalogue, "Crafting", 2_951_373);
    EqualDecimal(465_000m, crafting.ExperiencePerHour, "Crafting rate");
    Equal("Black dragonhide bodies", crafting.Method, "Crafting method");
    EqualDecimal(3m / 258m, Resource(crafting, 2509).QuantityPerExperience, "black leather per XP");
    EqualDecimal(1m / 258m, Resource(crafting, 2503).QuantityPerExperience, "black bodies per XP");

    var smithing = TrainingBand(catalogue, "Smithing", 13_034_431);
    EqualDecimal(410_000m, smithing.ExperiencePerHour, "Smithing 99+ rate");
    Equal("Solo Blast Furnace gold", smithing.Method, "Smithing method");
    EqualDecimal(72_000m, smithing.Economics!.FixedGpPerHour, "Blast Furnace hourly fee");
    EqualDecimal(10m, Resource(smithing, 12625).QuantityPerHour, "stamina potions per hour");

    var herblore = TrainingBand(catalogue, "Herblore", 2_192_818);
    EqualDecimal(450_000m, herblore.ExperiencePerHour, "Herblore rate");
    Equal("Saradomin brews", herblore.Method, "Herblore method");
    Equal(
        "3002|6685|6693|21163",
        string.Join('|', herblore.Economics!.Resources.Select(resource => resource.ItemId).Order()),
        "Herblore item IDs");
    EqualDecimal(
        0.90m / 180m,
        Resource(herblore, 6693).QuantityPerExperience,
        "Prescription goggles secondary consumption");
    EqualDecimal(
        0.15m / 10m / 180m,
        Resource(herblore, 21163).QuantityPerExperience,
        "Alchemist's amulet charge consumption");
    EqualDecimal(
        (3m + 0.15m) / 4m / 180m,
        Resource(herblore, 6685).QuantityPerExperience,
        "four-dose Alchemist's amulet brew output");
    True(
        catalogue.Skills.Single(skill => skill.Skill == "Herblore").Note?
            .Contains("Prescription goggles", StringComparison.Ordinal) == true,
        "Herblore note should disclose Prescription goggles");
    True(
        catalogue.Skills.Single(skill => skill.Skill == "Herblore").Note?
            .Contains("Alchemist's amulet", StringComparison.Ordinal) == true,
        "Herblore note should disclose Alchemist's amulet");

    var fletching = TrainingBand(catalogue, "Fletching", 5_346_332);
    EqualDecimal(1_000_000m, fletching.ExperiencePerHour, "Fletching rate");
    Equal("Amethyst darts", fletching.Method, "Fletching method");
    EqualDecimal(1m / 21m, Resource(fletching, 25853).QuantityPerExperience, "amethyst tips per XP");

    var firemaking = TrainingBand(catalogue, "Firemaking", 13_034_431);
    EqualDecimal(623_700m * 1.025m, firemaking.ExperiencePerHour, "Firemaking rate");
    Equal("Rosewood logs - bow burning", firemaking.Method, "Firemaking method");
    EqualDecimal(
        1m / (420m * 1.025m),
        Resource(firemaking, 32910).QuantityPerExperience,
        "rosewood logs per XP");

    foreach (var band in new[] { prayer, cooking, crafting, smithing, herblore, fletching, firemaking })
        True(band.Economics is { IsComplete: true }, $"{band.Method} should be fully modelled");
}

static void HerbloreEquipmentEconomics()
{
    var catalogue = new MainEhpCatalogue();
    var definition = catalogue.Skills.Single(skill => skill.Skill == "Herblore");
    var prices = new Dictionary<int, ItemPrice>
    {
        [3002] = new ItemPrice(3002, 1_000, 900, null, null),
        [6693] = new ItemPrice(6693, 2_000, 1_900, null, null),
        [21163] = new ItemPrice(21163, 3_000, 2_900, null, null),
        [6685] = new ItemPrice(6685, 600, 500, null, null)
    };

    var result = new TrainingPlanCalculator().Calculate(
        definition,
        2_192_818,
        2_642_818,
        prices);

    EqualDecimal(1m, result.Hours, "one hour of Saradomin brews");
    EqualDecimal(
        -6_147_812.5m,
        result.NetGp ?? 0m,
        "equipment-adjusted brew GP per hour",
        0.01m);
    EqualDecimal(
        -6_147_812.5m,
        result.AverageGpPerHour ?? 0m,
        "equipment-adjusted displayed GP per hour",
        0.01m);
}

static void HerbloreAlternativeMethods()
{
    var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Herblore");
    Equal(
        "main-ehp|super-restores|1t-extended-super-antifires",
        string.Join('|', definition.AvailableMethods.Select(method => method.Id)),
        "Herblore method IDs");

    var restores = definition.ResolveMethod("super-restores");
    var restoreBand = restores.Bands.Last();
    Equal(368_599L, restoreBand.StartExperience, "Super restores unlock XP");
    EqualDecimal(356_250m, restoreBand.ExperiencePerHour, "Super restores XP/hour");
    EqualDecimal(1m / 142.5m, Resource(restoreBand, 3004).QuantityPerExperience, "unfinished snapdragon per XP");
    EqualDecimal(0.9m / 142.5m, Resource(restoreBand, 223).QuantityPerExperience, "goggle-adjusted eggs per XP");
    EqualDecimal(0.015m / 142.5m, Resource(restoreBand, 21163).QuantityPerExperience, "restore amulet charges per XP");
    EqualDecimal(0.7875m / 142.5m, Resource(restoreBand, 3024).QuantityPerExperience, "four-dose restores per XP");

    var extended = definition.ResolveMethod("1t-extended-super-antifires");
    var extendedBand = extended.Bands.Last();
    Equal(11_805_606L, extendedBand.StartExperience, "Extended super antifires unlock XP");
    EqualDecimal(840_000m, extendedBand.ExperiencePerHour, "Extended super antifires XP/hour");
    EqualDecimal(1m / 160m, Resource(extendedBand, 21978).QuantityPerExperience, "super antifire(4) per XP");
    EqualDecimal(3.6m / 160m, Resource(extendedBand, 11994).QuantityPerExperience, "goggle-adjusted shards per XP");
    EqualDecimal(1m / 160m, Resource(extendedBand, 22209).QuantityPerExperience, "extended super antifire(4) per XP");
    True(
        extendedBand.Economics!.Resources.All(resource => resource.ItemId != 21163),
        "Alchemist's amulet must not apply to extended super antifires");

    True(
        definition.AvailableMethods
            .SelectMany(method => method.Bands)
            .SelectMany(band => band.Economics?.Resources ?? [])
            .All(resource => resource.ItemId != 6687),
        "Herblore methods must not sell three-dose potions");

    var calculator = new TrainingPlanCalculator();
    var restorePlan = calculator.Calculate(
        definition,
        0,
        TrainingPlanCalculator.MaximumExperience,
        new Dictionary<int, ItemPrice>(),
        methodId: restores.Id);
    var extendedPlan = calculator.Calculate(
        definition,
        0,
        TrainingPlanCalculator.MaximumExperience,
        new Dictionary<int, ItemPrice>(),
        methodId: extended.Id);

    True(restorePlan.Hours is > 562m and < 563m, "Super restore 0-to-200m hours");
    True(extendedPlan.Hours is > 251m and < 252m, "Extended super antifire 0-to-200m hours");

    var restoreHour = calculator.Calculate(
        definition,
        368_599,
        724_849,
        new Dictionary<int, ItemPrice>
        {
            [3004] = new ItemPrice(3004, 1_000, 900, null, null),
            [223] = new ItemPrice(223, 2_000, 1_900, null, null),
            [21163] = new ItemPrice(21163, 3_000, 2_900, null, null),
            [3024] = new ItemPrice(3024, 600, 500, null, null)
        },
        methodId: restores.Id);
    EqualDecimal(1m, restoreHour.Hours, "one hour of Super restores");
    EqualDecimal(-6_147_812.5m, restoreHour.NetGp ?? 0m, "Super restore hourly economics", 0.01m);

    var extendedHour = calculator.Calculate(
        definition,
        11_805_606,
        12_645_606,
        new Dictionary<int, ItemPrice>
        {
            [21978] = new ItemPrice(21978, 1_000, 900, null, null),
            [11994] = new ItemPrice(11994, 200, 190, null, null),
            [22209] = new ItemPrice(22209, 2_600, 2_500, null, null)
        },
        methodId: extended.Id);
    EqualDecimal(1m, extendedHour.Hours, "one hour of extended super antifires");
    EqualDecimal(3_832_500m, extendedHour.NetGp ?? 0m, "extended super antifire hourly economics", 0.01m);
}

static void PracticalBuyableMethods()
{
    var catalogue = new MainEhpCatalogue();
    var calculator = new TrainingPlanCalculator();
    var emptyPrices = new Dictionary<int, ItemPrice>();

    var smithing = catalogue.Skills.Single(skill => skill.Skill == "Smithing");
    Equal(
        "main-ehp|adamant-platebodies|rune-2h-swords",
        string.Join('|', smithing.AvailableMethods.Select(method => method.Id)),
        "Smithing method IDs");
    var adamant = smithing.ResolveMethod("adamant-platebodies").Bands.Last();
    Equal(4_382_299L, adamant.StartExperience, "Adamant platebody unlock XP");
    EqualDecimal(260_400m, adamant.ExperiencePerHour, "Adamant platebody base rate");
    EqualDecimal(5m / 312.5m, Resource(adamant, 2361).QuantityPerExperience, "adamantite bars per XP");
    EqualDecimal(1m / 312.5m, Resource(adamant, 1123).QuantityPerExperience, "adamant platebodies per XP");
    var uniformAdamant = calculator.Calculate(
        smithing,
        4_382_299,
        4_707_299,
        emptyPrices,
        methodId: "adamant-platebodies",
        configuration: new Dictionary<string, string>
        {
            ["smiths-uniform"] = bool.TrueString
        });
    EqualDecimal(325_000m, uniformAdamant.BaseRate, "Adamant platebody uniform rate");
    EqualDecimal(
        5m / 312.5m,
        Resource(uniformAdamant.Method.Bands.Last(), 2361).QuantityPerExperience,
        "Smiths' uniform changes speed rather than bar consumption");

    var rune = smithing.ResolveMethod("rune-2h-swords").Bands.Last();
    Equal(13_034_431L, rune.StartExperience, "Rune 2h unlock XP");
    EqualDecimal(217_000m, rune.ExperiencePerHour, "Rune 2h base rate");
    EqualDecimal(3m / 225m, Resource(rune, 2363).QuantityPerExperience, "runite bars per XP");
    EqualDecimal(1m / 225m, Resource(rune, 1319).QuantityPerExperience, "rune 2h swords per XP");

    var construction = catalogue.Skills.Single(skill => skill.Skill == "Construction");
    var doors = construction.ResolveMethod("oak-dungeon-doors").Bands.Last();
    Equal(1_210_421L, doors.StartExperience, "Oak dungeon door unlock XP");
    EqualDecimal(550_000m, doors.ExperiencePerHour, "Oak dungeon door rate");
    EqualDecimal(1m / 60m, Resource(doors, 8778).QuantityPerExperience, "oak planks per XP");
    EqualDecimal(1_250m / 25m / 60m, doors.Economics!.FixedGpPerExperience, "Oak dungeon door servant fee");

    var prayer = catalogue.Skills.Single(skill => skill.Skill == "Prayer");
    var dragonBones = prayer.ResolveMethod("dragon-bones").Bands.Single();
    EqualDecimal(642_600m, dragonBones.ExperiencePerHour, "Gilded Altar dragon bone rate");
    EqualDecimal(1m / 252m, Resource(dragonBones, 536).QuantityPerExperience, "Gilded dragon bones per XP");
    var chaosDragonBones = calculator.Calculate(
        prayer,
        0,
        504_000,
        emptyPrices,
        methodId: "dragon-bones",
        configuration: new Dictionary<string, string>
        {
            ["offering-location"] = "chaos-altar"
        });
    EqualDecimal(504_000m, chaosDragonBones.BaseRate, "Chaos Altar dragon bone rate");
    EqualDecimal(
        1m / 504m,
        Resource(chaosDragonBones.Method.Bands.Single(), 536).QuantityPerExperience,
        "Chaos Altar effective dragon bones per XP");

    var crafting = catalogue.Skills.Single(skill => skill.Skill == "Crafting");
    var airStaves = crafting.ResolveMethod("air-battlestaves").Bands.Last();
    Equal(496_254L, airStaves.StartExperience, "Air battlestaff unlock XP");
    EqualDecimal(336_875m, airStaves.ExperiencePerHour, "Air battlestaff rate");
    EqualDecimal(1m / 137.5m, Resource(airStaves, 1391).QuantityPerExperience, "battlestaves per XP");
    EqualDecimal(1m / 137.5m, Resource(airStaves, 573).QuantityPerExperience, "air orbs per XP");
    EqualDecimal(1m / 137.5m, Resource(airStaves, 1397).QuantityPerExperience, "air battlestaves per XP");

    var fletching = catalogue.Skills.Single(skill => skill.Skill == "Fletching");
    var adamantDarts = fletching.ResolveMethod("adamant-darts").Bands.Last();
    Equal(737_627L, adamantDarts.StartExperience, "Adamant dart unlock XP");
    EqualDecimal(300_000m, adamantDarts.ExperiencePerHour, "Adamant dart rate");
    EqualDecimal(1m / 15m, Resource(adamantDarts, 823).QuantityPerExperience, "adamant dart tips per XP");
    EqualDecimal(1m / 15m, Resource(adamantDarts, 314).QuantityPerExperience, "adamant dart feathers per XP");
    EqualDecimal(1m / 15m, Resource(adamantDarts, 810).QuantityPerExperience, "adamant darts per XP");
}

static void RunecraftAlternativeMethods()
{
    var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Runecraft");
    var calculator = new TrainingPlanCalculator();
    var emptyPrices = new Dictionary<int, ItemPrice>();

    Equal(
        "main-ehp|solo-lava-runes|solo-aether-runes",
        string.Join('|', definition.AvailableMethods.Select(method => method.Id)),
        "Runecraft method IDs");

    var mud = definition.ResolveMethod("main-ehp").Bands.Last();
    EqualDecimal(98_200m, mud.ExperiencePerHour, "solo mud 99 rate");
    EqualDecimal(99m / 598.5m, Resource(mud, 4698).QuantityPerExperience, "Raiments mud output");
    var noRaimentsMud = calculator.Calculate(
        definition,
        13_034_431,
        13_132_631,
        emptyPrices,
        methodId: "main-ehp",
        configuration: new Dictionary<string, string>
        {
            ["raiments-of-the-eye"] = bool.FalseString
        });
    EqualDecimal(98_200m, noRaimentsMud.BaseRate, "Raiments do not alter mud XP/hour");
    EqualDecimal(
        1m / 9.5m,
        Resource(noRaimentsMud.Method.Bands.Last(), 4698).QuantityPerExperience,
        "mud output without Raiments");

    var lavaMethod = definition.ResolveMethod("solo-lava-runes");
    Equal(6_291L, lavaMethod.Bands[1].StartExperience, "solo lava unlock XP");
    EqualDecimal(40_000m, lavaMethod.Bands[1].ExperiencePerHour, "solo lava entry rate");
    var lava85 = lavaMethod.Bands.Last();
    Equal(3_258_594L, lava85.StartExperience, "solo lava colossal-pouch unlock XP");
    EqualDecimal(102_100m, lava85.ExperiencePerHour, "solo lava colossal-pouch rate");
    EqualDecimal(1m / 10.5m, Resource(lava85, 7936).QuantityPerExperience, "lava essence per XP");
    EqualDecimal(1m / 10.5m, Resource(lava85, 557).QuantityPerExperience, "lava earth runes per XP");
    EqualDecimal(99m / 661.5m, Resource(lava85, 4699).QuantityPerExperience, "Raiments lava output");

    var aetherMethod = definition.ResolveMethod("solo-aether-runes");
    var aether90 = aetherMethod.Bands.Single(band => band.StartExperience == 5_346_332);
    EqualDecimal(99_000m, aether90.ExperiencePerHour, "solo aether level-90 rate");
    EqualDecimal(1m / 20m, Resource(aether90, 7936).QuantityPerExperience, "aether essence per XP");
    EqualDecimal(1m / 20m, Resource(aether90, 566).QuantityPerExperience, "aether soul runes per XP");
    EqualDecimal(99m / 1_260m, Resource(aether90, 30771).QuantityPerExperience, "Raiments catalyst cost");
    EqualDecimal(99m / 1_260m, Resource(aether90, 30843).QuantityPerExperience, "Raiments aether output");
    EqualDecimal(0.125m / 1_260m, Resource(aether90, 2552).QuantityPerExperience, "aether rings of dueling per XP");
    var aether99 = aetherMethod.Bands.Last();
    EqualDecimal(102_000m, aether99.ExperiencePerHour, "solo aether level-99 rate");
    True(
        aether99.Economics!.Resources.All(resource => resource.ItemId is not 556 and not 564),
        "Runecraft cape removes aether pouch-repair runes");

    var noRaimentsAether = calculator.Calculate(
        definition,
        5_346_332,
        5_445_332,
        emptyPrices,
        methodId: "solo-aether-runes",
        configuration: new Dictionary<string, string>
        {
            ["raiments-of-the-eye"] = bool.FalseString
        });
    var noRaimentsAetherBand = noRaimentsAether.Method.Bands
        .Single(band => band.StartExperience == 5_346_332);
    EqualDecimal(99_000m, noRaimentsAether.BaseRate, "Raiments do not alter aether XP/hour");
    EqualDecimal(1m / 20m, Resource(noRaimentsAetherBand, 30771).QuantityPerExperience, "base catalyst cost");
    EqualDecimal(1m / 20m, Resource(noRaimentsAetherBand, 30843).QuantityPerExperience, "base aether output");
}

static void PhaseTwoMethodCatalogue()
{
    var catalogue = new MainEhpCatalogue();

    var woodcutting = TrainingBand(catalogue, "Woodcutting", 814_445);
    EqualDecimal(194_022m, woodcutting.ExperiencePerHour, "Woodcutting level-71 rate");
    Equal("1.5t teaks - crystal felling axe", woodcutting.Method, "Woodcutting method");
    EqualDecimal(
        2_091_504m,
        TotalResourceQuantity(catalogue, "Woodcutting", 28157),
        "Woodcutting Forester's rations",
        0.001m);
    EqualDecimal(
        100m,
        TotalResourceQuantity(catalogue, "Woodcutting", 23959),
        "Woodcutting enhanced seeds",
        0.001m);
    True(
        catalogue.Skills.Single(skill => skill.Skill == "Woodcutting").Note?.Contains("14,953") == true,
        "Woodcutting note should retain the consumed shard total");
    True(
        catalogue.Skills.Single(skill => skill.Skill == "Woodcutting")
            .Bands.SelectMany(band => band.Economics?.Resources ?? [])
            .All(resource => resource.Direction == TrainingFlowDirection.Input),
        "dropped teak logs should not be valued as outputs");

    var fishing = TrainingBand(catalogue, "Fishing", 814_445);
    EqualDecimal(132_800m, fishing.ExperiencePerHour, "Fishing rate");
    Equal("2t swordfish and tuna - crystal harpoon", fishing.Method, "Fishing method");
    EqualDecimal(
        33m,
        TotalResourceQuantity(catalogue, "Fishing", 23959),
        "Fishing enhanced seeds",
        0.001m);
    True(
        catalogue.Skills.Single(skill => skill.Skill == "Fishing").Note?.Contains("4,894") == true,
        "Fishing note should retain the consumed shard total");

    var mining = TrainingBand(catalogue, "Mining", 393_485);
    EqualDecimal(106_540m, mining.ExperiencePerHour, "Mining first granite rate");
    Equal("3t4g granite - infernal pickaxe", mining.Method, "Mining method");
    EqualDecimal(
        1m / 960_000m,
        Resource(mining, 11920).QuantityPerExperience,
        "dragon pickaxes per Mining XP");
    Equal(TrainingFlowDirection.Input, Resource(mining, 11920).Direction, "dragon pickaxe direction");

    var hunter = TrainingBand(catalogue, "Hunter", 992_895);
    EqualDecimal(265_000m, hunter.ExperiencePerHour, "Hunter rate");
    Equal("Black chinchompas - shooting alt", hunter.Method, "Hunter method");
    EqualDecimal(1m / 315m, Resource(hunter, 11959).QuantityPerExperience, "black chins per Hunter XP");
    Equal(TrainingFlowDirection.Output, Resource(hunter, 11959).Direction, "black chin direction");
    True(Resource(hunter, 11959).SubjectToGeTax, "black chins should be GE taxed");

    var runecraft75 = TrainingBand(catalogue, "Runecraft", 1_210_421);
    EqualDecimal(74_500m, runecraft75.ExperiencePerHour, "Runecraft level-75 rate");
    EqualDecimal(50m / 475m, Resource(runecraft75, 7936).QuantityPerExperience, "level-75 essence per XP");
    EqualDecimal(80m / 475m, Resource(runecraft75, 4698).QuantityPerExperience, "level-75 mud runes per XP");
    EqualDecimal(0.2m / 475m, Resource(runecraft75, 5521).QuantityPerExperience, "level-75 necklaces per XP");
    EqualDecimal(2.1m / 475m, Resource(runecraft75, 9075).QuantityPerExperience, "level-75 astrals per XP");

    var runecraft85 = TrainingBand(catalogue, "Runecraft", 3_258_594);
    EqualDecimal(96_900m, runecraft85.ExperiencePerHour, "Runecraft level-85 rate");
    EqualDecimal(63m / 598.5m, Resource(runecraft85, 7936).QuantityPerExperience, "level-85 essence per XP");
    EqualDecimal(99m / 598.5m, Resource(runecraft85, 4698).QuantityPerExperience, "level-85 mud runes per XP");
    EqualDecimal(2.125m / 598.5m, Resource(runecraft85, 9075).QuantityPerExperience, "level-85 astrals per XP");
    EqualDecimal(0.25m / 598.5m, Resource(runecraft85, 556).QuantityPerExperience, "level-85 air runes per XP");
    EqualDecimal(0.125m / 598.5m, Resource(runecraft85, 564).QuantityPerExperience, "level-85 cosmic runes per XP");

    var runecraft99 = TrainingBand(catalogue, "Runecraft", 13_034_431);
    EqualDecimal(98_200m, runecraft99.ExperiencePerHour, "Runecraft level-99 rate");
    Equal("Solo mud runes", runecraft99.Method, "Runecraft method");
    True(
        runecraft99.Economics!.Resources.All(resource => resource.ItemId is not 556 and not 564),
        "Runecraft cape should remove NPC Contact rune costs");

    foreach (var band in new[] { woodcutting, fishing, mining, hunter, runecraft75, runecraft85, runecraft99 })
        True(band.Economics is { IsComplete: true }, $"{band.Method} should expose reviewed economics");
}

static void PhaseTwoTrainingCalculations()
{
    var catalogue = new MainEhpCatalogue();
    var calculator = new TrainingPlanCalculator();
    var prices = new Dictionary<int, ItemPrice>
    {
        [23959] = Quote(23959, 3_000_000),
        [28157] = Quote(28157, 50),
        [11920] = Quote(11920, 1_000_000),
        [11959] = Quote(11959, 3_000),
        [7936] = Quote(7936, 1),
        [557] = Quote(557, 5),
        [5521] = Quote(5521, 1_000),
        [9075] = Quote(9075, 200),
        [556] = Quote(556, 5),
        [564] = Quote(564, 100),
        [4698] = Quote(4698, 100)
    };

    var woodcutting = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Woodcutting"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    Equal(199_977_594L, woodcutting.PricedExperience, "priced Woodcutting XP");
    EqualDecimal(
        -(2_091_504m * 50m + 100m * 3_000_000m),
        woodcutting.NetGp ?? 0m,
        "Woodcutting resource cost",
        0.01m);
    True(!woodcutting.IsFullyPriced, "early Woodcutting should remain visibly unpriced");

    var fishing = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Fishing"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    Equal(199_185_555L, fishing.PricedExperience, "priced Fishing XP");
    EqualDecimal(
        -(33m * 3_000_000m),
        fishing.NetGp ?? 0m,
        "Fishing crystal-charge cost",
        0.01m);
    True(!fishing.IsFullyPriced, "early Fishing should remain visibly unpriced");

    var mining = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Mining"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    Equal(199_606_515L, mining.PricedExperience, "priced Mining XP");
    EqualDecimal(
        -(199_606_515m / 960_000m * 1_000_000m),
        mining.NetGp ?? 0m,
        "Mining infernal-pickaxe cost",
        0.01m);
    True(!mining.IsFullyPriced, "early Mining should remain visibly unpriced");

    var hunter = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Hunter"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    Equal(199_007_105L, hunter.PricedExperience, "priced Hunter XP");
    EqualDecimal(
        199_007_105m / 315m * 2_940m,
        hunter.NetGp ?? 0m,
        "Hunter black-chin revenue after tax",
        0.01m);
    True(hunter.NetGp > 0m, "black chinchompas should produce profit");

    var runecraft = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Runecraft"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    Equal(198_789_579L, runecraft.PricedExperience, "priced Runecraft XP");
    True(runecraft.NetGp > 0m, "solo mud runes should produce profit with the test market");
    True(!runecraft.HasMissingPrice, "all solo mud-rune resources should be priced");
    True(!runecraft.IsFullyPriced, "early Runecraft should remain visibly unpriced");
}

static void PhaseThreeMethodCatalogue()
{
    var catalogue = new MainEhpCatalogue();

    var agilityDefinition = catalogue.Skills.Single(skill => skill.Skill == "Agility");
    var agility = TrainingBand(catalogue, "Agility", 0);
    EqualDecimal(98_500m, agility.ExperiencePerHour, "Agility rate");
    Equal("Hallowed Sepulchre - Grand Coffin", agility.Method, "Agility method");
    EqualDecimal(0.5m / 11_700m, Resource(agility, 12625).QuantityPerExperience, "stamina potions per XP");
    EqualDecimal(1m / 200m / 11_700m, Resource(agility, 24844).QuantityPerExperience, "rings per XP");
    EqualDecimal(20m / 11_700m, Resource(agility, 565).QuantityPerExperience, "blood runes per XP");
    EqualDecimal(0.15m / 11_700m, Resource(agility, 10925).QuantityPerExperience, "Sanfew serum per XP");
    EqualDecimal(2_125m / 11_700m, agility.Economics!.FixedGpOutputPerExperience, "coins per XP");
    True(agilityDefinition.Note?.Contains("17,095") == true, "Agility note should retain coffin total");
    True(agilityDefinition.Note?.Contains("3,419,000") == true, "Agility note should retain Thieving XP");
    True(agilityDefinition.Note?.Contains("only the Grand Coffin", StringComparison.OrdinalIgnoreCase) == true, "Agility note should disclose looting scope");
    True(agility.Economics is { IsComplete: true }, "Grand Coffin economics should be fully modelled");

    var thieving = TrainingBand(catalogue, "Thieving", 0);
    EqualDecimal(260_000m, thieving.ExperiencePerHour, "Gem knights rate");
    Equal("Gem knights", thieving.Method, "Thieving method");
    EqualDecimal(
        (182m / 195m) * 5m * 1.8m / 260_000m / 103.4m,
        Resource(thieving, 6571).QuantityPerExperience,
        "Gem knights Tokkul-to-onyx output per XP");
    Equal(1, thieving.Economics?.Resources.Count ?? 0, "Gem knights price only Tokkul conversion");
    True(thieving.Economics is { IsComplete: true }, "Gem knights Tokkul projection should be priced");

    var farming = TrainingBand(catalogue, "Farming", 6_517_253);
    Equal(
        "16000|364000|575000|841000|1222000|1428000|2063000|2475000|2611000|2669000",
        string.Join(
            '|',
            catalogue.Skills.Single(skill => skill.Skill == "Farming")
                .Bands.Select(band => band.ExperiencePerHour)),
        "Farming rate progression");
    EqualDecimal(2_669_000m, farming.ExperiencePerHour, "Farming level-92 rate");
    Equal("Efficient tree runs - magic + dragonfruit", farming.Method, "Farming method");
    True(farming.Economics is { IsComplete: true }, "Farming economics should be complete");
    const decimal rosewoodTreesPerDay = 1m;
    const decimal redwoodTreesPerDay = 0.225m;
    const decimal farmingExperiencePerDay =
        6m * 13_913.8m
        + 6m * 17_475m
        + rosewoodTreesPerDay * 23_352m
        + 12_225.5m
        + 14_334m
        + redwoodTreesPerDay * 22_680m;
    EqualDecimal(
        6m / farmingExperiencePerDay,
        Resource(farming, 5374).QuantityPerExperience,
        "Magic saplings per Farming XP");
    EqualDecimal(
        240m / farmingExperiencePerDay,
        Resource(farming, 5974).QuantityPerExperience,
        "coconuts per Farming XP");
    EqualDecimal(
        (8m * rosewoodTreesPerDay + 6m * redwoodTreesPerDay) / farmingExperiencePerDay,
        Resource(farming, 22929).QuantityPerExperience,
        "dragonfruit protection per Farming XP");
    True(
        catalogue.Skills.Single(skill => skill.Skill == "Farming").Note?
            .Contains("not harvested", StringComparison.OrdinalIgnoreCase) == true,
        "Farming note should disclose excluded harvest value");

    var sailingDefinition = catalogue.Skills.Single(skill => skill.Skill == "Sailing");
    var sailing = TrainingBand(catalogue, "Sailing", 0);
    EqualDecimal(240_000m, sailing.ExperiencePerHour, "Gwenith Glide rate");
    Equal("Gwenith Glide - rosewood hull", sailing.Method, "Sailing method");
    EqualDecimal(48.12m, Resource(sailing, 12695).QuantityPerHour, "regular potions per hour");
    EqualDecimal(48.12m, Resource(sailing, 23685).QuantityPerHour, "divine potions per hour");
    Equal(TrainingFlowDirection.Input, Resource(sailing, 12695).Direction, "regular potion direction");
    Equal(TrainingFlowDirection.Output, Resource(sailing, 23685).Direction, "divine potion direction");
    True(Resource(sailing, 23685).SubjectToGeTax, "divine potions should be GE taxed");
    True(sailingDefinition.Note?.Contains("16,040") == true, "Sailing note should retain the shard total");
    True(sailingDefinition.Note?.Contains("40,100") == true, "Sailing note should retain the potion total");
    True(
        !sailingDefinition.Bands.Any(band => band.Method.Contains("Spin Flax", StringComparison.OrdinalIgnoreCase)),
        "Sailing should exclude multiskilling");
}

static void PhaseThreeTrainingCalculations()
{
    var catalogue = new MainEhpCatalogue();
    var calculator = new TrainingPlanCalculator();

    var agilityPrices = new Dictionary<int, ItemPrice>
    {
        [12625] = Quote(12625, 3_000),
        [24844] = Quote(24844, 4_000_000),
        [1319] = Quote(1319, 40_000),
        [1127] = Quote(1127, 40_000),
        [563] = Quote(563, 100),
        [565] = Quote(565, 300),
        [566] = Quote(566, 400),
        [9144] = Quote(9144, 100),
        [7946] = Quote(7946, 200),
        [10925] = Quote(10925, 20_000),
        [5295] = Quote(5295, 30_000)
    };
    var agility = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Agility"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        agilityPrices);
    var reviewedCoffins = TrainingPlanCalculator.MaximumExperience / 11_700m;
    const decimal expectedNetGpPerCoffin =
        19_600m + 3_920m + 3_920m + 1_960m + 5_880m + 7_840m + 1_960m
        + 78.4m + 2_940m + 4_410m + 2_125m - 1_500m;
    EqualDecimal(2_030.4569m, agility.Hours, "Grand Coffin 0-200m hours", 0.0001m);
    EqualDecimal(
        reviewedCoffins * 0.5m,
        TotalResourceQuantity(catalogue, "Agility", 12625),
        "Grand Coffin stamina potions",
        0.0001m);
    EqualDecimal(
        reviewedCoffins * expectedNetGpPerCoffin,
        agility.NetGp ?? 0m,
        "Grand Coffin expected profit",
        0.01m);
    True(agility.IsFullyPriced, "Grand Coffin projection should be fully priced");

    var thieving = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Thieving"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        new Dictionary<int, ItemPrice> { [6571] = Quote(6571, 2_600_000) });
    EqualDecimal(769.2308m, thieving.Hours, "Gem knights 0-200m hours", 0.0001m);
    var expectedTokkul = TrainingPlanCalculator.MaximumExperience
                         / 103.4m
                         * (182m / 195m)
                         * 5m
                         * 1.8m;
    var expectedOnyx = expectedTokkul / 260_000m;
    EqualDecimal(
        expectedOnyx * 2_548_000m,
        thieving.NetGp ?? 0m,
        "Gem knights Tokkul-to-onyx value",
        0.01m);
    True(thieving.IsFullyPriced, "Gem knights Tokkul projection should be fully priced");

    var prices = new Dictionary<int, ItemPrice>
    {
        [12695] = Quote(12695, 14_000),
        [23685] = Quote(23685, 18_000)
    };
    var sailingDefinition = catalogue.Skills.Single(skill => skill.Skill == "Sailing");
    var sailing = calculator.Calculate(
        sailingDefinition,
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);

    EqualDecimal(833.3333m, sailing.Hours, "Gwenith Glide 0-200m hours", 0.0001m);
    EqualDecimal(
        40_100m,
        TotalResourceQuantity(catalogue, "Sailing", 12695),
        "Gwenith regular potions",
        0.0001m);
    EqualDecimal(
        40_100m,
        TotalResourceQuantity(catalogue, "Sailing", 23685),
        "Gwenith divine potions",
        0.0001m);
    EqualDecimal(
        40_100m * (18_000m - 360m - 14_000m),
        sailing.NetGp ?? 0m,
        "Gwenith potion profit after tax",
        0.01m);
    True(sailing.IsFullyPriced, "Gwenith projection should be fully priced");

    var fasterSailing = calculator.Calculate(
        sailingDefinition,
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices,
        300_000m);
    EqualDecimal(666.6667m, fasterSailing.Hours, "personal Sailing rate hours", 0.0001m);
    EqualDecimal(
        (sailing.NetGp ?? 0m) * 0.8m,
        fasterSailing.NetGp ?? 0m,
        "hourly Sailing resources scale with hours",
        0.01m);
}

static void FarmingTrainingCalculations()
{
    var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Farming");
    Equal(
        "main-ehp|magic-palm-tree-runs",
        string.Join('|', definition.AvailableMethods.Select(method => method.Id)),
        "Farming method IDs");
    Equal(
        "Magic + dragonfruit tree runs|Magic + palm tree runs",
        string.Join('|', definition.AvailableMethods.Select(method => method.Name)),
        "Farming method dropdown names");

    var palmMethod = definition.ResolveMethod("magic-palm-tree-runs");
    var palmRates = palmMethod.Bands
        .Where(band => band.StartExperience >= 2_192_818)
        .Select(band => band.ExperiencePerHour)
        .ToArray();
    EqualDecimal(1_995_973.1502m, palmRates[0], "level-81 palm rate", 0.0001m);
    EqualDecimal(2_136_088.6575m, palmRates[1], "level-85 palm rate", 0.0001m);
    EqualDecimal(2_194_240.6680m, palmRates[2], "level-92 palm rate", 0.0001m);

    var palmBand = palmMethod.Bands.Last();
    const decimal palmExperiencePerDay =
        6m * 13_913.8m
        + 6m * 10_260.6m
        + 23_352m
        + 12_225.5m
        + 14_334m
        + 0.225m * 22_680m;
    EqualDecimal(
        6m / palmExperiencePerDay,
        Resource(palmBand, 5502).QuantityPerExperience,
        "palm saplings per Farming XP");
    EqualDecimal(
        90m / palmExperiencePerDay,
        Resource(palmBand, 5972).QuantityPerExperience,
        "papaya protection per Farming XP");
    True(
        palmBand.Economics!.Resources.All(resource => resource.ItemId != 22866),
        "palm route should not buy dragonfruit saplings");

    var prices = definition.AvailableMethods
        .SelectMany(method => method.Bands)
        .Where(band => band.Economics is not null)
        .SelectMany(band => band.Economics!.Resources)
        .Select(resource => resource.ItemId)
        .Distinct()
        .ToDictionary(itemId => itemId, itemId => Quote(itemId, 100));
    var calculator = new TrainingPlanCalculator();

    var pricedRoute = calculator.Calculate(
        definition,
        32_500,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    Equal(
        TrainingPlanCalculator.MaximumExperience - 32_500,
        pricedRoute.PricedExperience,
        "priced Farming XP");
    True(pricedRoute.IsFullyPriced, "every tree-run band should be fully priced");
    True(!pricedRoute.HasMissingPrice, "reviewed Farming inputs should all resolve");
    True(pricedRoute.NetGp < 0m, "tree runs should cost GP");

    var palmRoute = calculator.Calculate(
        definition,
        32_500,
        TrainingPlanCalculator.MaximumExperience,
        prices,
        methodId: palmMethod.Id);
    True(palmRoute.IsFullyPriced, "palm tree-run bands should be fully priced");
    True(!palmRoute.HasMissingPrice, "palm tree-run inputs should all resolve");
    True(palmRoute.Hours > pricedRoute.Hours, "lower-XP palm route should require more active hours");

    var fullRoute = calculator.Calculate(
        definition,
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    True(!fullRoute.IsFullyPriced, "quest XP should remain visibly unpriced");
    Equal(32_500L, fullRoute.ExperienceRemaining - fullRoute.PricedExperience, "unpriced quest XP");
}

static void CombatMethodCatalogue()
{
    var catalogue = new MainEhpCatalogue();
    True(
        catalogue.Skills.All(skill => skill.Skill is not "Attack" and not "Strength" and not "Hitpoints"),
        "zero-time melee and Hitpoints skills should be omitted from the XP Planner catalogue");

    var defence = TrainingBand(catalogue, "Defence", 0);
    EqualDecimal(405_000m, defence.ExperiencePerHour, "Defence rate");
    Equal("Black Chinchompas & Cannon - Defensive", defence.Method, "Defence method");
    EqualDecimal(1_500m / 405_000m, Resource(defence, 11959).QuantityPerExperience, "Defence chins per XP");
    EqualDecimal(6_000m / 405_000m, Resource(defence, 2).QuantityPerExperience, "Defence cannonballs per XP");

    var rangedDefinition = catalogue.Skills.Single(skill => skill.Skill == "Ranged");
    var ranged = TrainingBand(catalogue, "Ranged", 0);
    True(rangedDefinition.IsZeroTime, "Ranged should contribute zero active hours");
    EqualDecimal(1_150_000m, ranged.ExperiencePerHour, "Ranged rate");
    Equal("Black Chinchompas & Cannon", ranged.Method, "Ranged method");
    EqualDecimal(1_866m / 1_150_000m, Resource(ranged, 11959).QuantityPerExperience, "Ranged chins per XP");
    EqualDecimal(6_000m / 1_150_000m, Resource(ranged, 2).QuantityPerExperience, "Ranged cannonballs per XP");

    var magicDefinition = catalogue.Skills.Single(skill => skill.Skill == "Magic");
    var magic = TrainingBand(catalogue, "Magic", 0);
    True(magicDefinition.IsZeroTime, "Magic should contribute zero active hours");
    EqualDecimal(330_000m, magic.ExperiencePerHour, "Magic reference rate");
    Equal("Ice Barrage", magic.Method, "Magic method");
    EqualDecimal(2m * 0.85m * 1_085m / 330_000m, Resource(magic, 565).QuantityPerExperience, "blood runes per Magic XP");
    EqualDecimal(4m * 0.85m * 1_085m / 330_000m, Resource(magic, 560).QuantityPerExperience, "death runes per Magic XP");

    var slayerDefinition = catalogue.Skills.Single(skill => skill.Skill == "Slayer");
    var slayer = TrainingBand(catalogue, "Slayer", 0);
    EqualDecimal(123_040m, slayer.ExperiencePerHour, "Slayer rate");
    True(slayer.Economics is { IsComplete: true }, "Slayer break-even economics should be explicit");
    Equal(1, slayerDefinition.ExperienceOutputs?.Count ?? 0, "Slayer secondary skill count");
    Equal("Magic", slayerDefinition.ExperienceOutputs![0].Skill, "Slayer secondary skill");
    EqualDecimal(
        163_136_972m / (6_578m * 28_397m),
        slayerDefinition.ExperienceOutputs[0].QuantityPerPrimaryExperience,
        "Magic XP per Slayer XP");
}

static void CombatDependencyCalculations()
{
    var catalogue = new MainEhpCatalogue();
    var calculator = new TrainingPlanCalculator();
    var prices = new Dictionary<int, ItemPrice>
    {
        [2] = Quote(2, 200),
        [11959] = Quote(11959, 3_000),
        [560] = Quote(560, 150),
        [565] = Quote(565, 300)
    };

    var defence = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Defence"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    EqualDecimal(493.8272m, defence.Hours, "Defence 0-200m hours", 0.0001m);
    EqualDecimal(
        -TrainingPlanCalculator.MaximumExperience
        * (1_500m / 405_000m * 3_000m + 6_000m / 405_000m * 200m),
        defence.NetGp ?? 0m,
        "Defence supply cost",
        0.01m);

    var ranged = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Ranged"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices);
    EqualDecimal(0m, ranged.Hours, "zero-time Ranged hours");
    EqualDecimal(
        -TrainingPlanCalculator.MaximumExperience
        * (1_866m / 1_150_000m * 3_000m + 6_000m / 1_150_000m * 200m),
        ranged.NetGp ?? 0m,
        "Ranged supply cost",
        0.01m);
    True(ranged.GpPerExperience < 0m, "Ranged should expose GP per XP");

    const long reviewedSlayerExperience = 6_578L * 28_397L;
    var slayer = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Slayer"),
        0,
        reviewedSlayerExperience,
        prices);
    EqualDecimal(0m, slayer.NetGp ?? decimal.MinValue, "Slayer GP/XP break-even");
    True(slayer.IsFullyPriced, "Slayer should be explicitly fully priced at break-even");
    EqualDecimal(163_136_972m, slayer.GeneratedExperience["Magic"], "generated Slayer Magic XP", 0.0001m);

    var magic = calculator.Calculate(
        catalogue.Skills.Single(skill => skill.Skill == "Magic"),
        0,
        TrainingPlanCalculator.MaximumExperience,
        prices,
        pendingExperienceCredit: (long)slayer.GeneratedExperience["Magic"]);
    const long expectedMagicRemaining = TrainingPlanCalculator.MaximumExperience - 163_136_972L;
    Equal(163_136_972L, magic.AppliedExperienceCredit, "applied Slayer Magic credit");
    Equal(expectedMagicRemaining, magic.ExperienceRemaining, "Magic XP left for Ice Barrage");
    Equal(0L, magic.StartExperience, "Magic profile start remains unchanged");
    EqualDecimal(0m, magic.Hours, "zero-time Magic hours");
    EqualDecimal(
        -expectedMagicRemaining
        * (2m * 0.85m * 1_085m / 330_000m * 300m
           + 4m * 0.85m * 1_085m / 330_000m * 150m),
        magic.NetGp ?? 0m,
        "Ice Barrage residual cost",
        0.01m);
}

static void TrainingRateOverride()
{
    var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Construction");
    var prices = new Dictionary<int, ItemPrice> { [8778] = Quote(8778, 431), [8782] = Quote(8782, 1_910) };
    var calculator = new TrainingPlanCalculator();
    var baseline = calculator.Calculate(definition, 0, 200_000_000, prices);
    var doubled = calculator.Calculate(definition, 0, 200_000_000, prices, 109_400m);

    EqualDecimal(baseline.Hours / 2m, doubled.Hours, "double-rate hours", 0.0001m);
    EqualDecimal(baseline.NetGp ?? 0m, doubled.NetGp ?? 0m, "rate override total GP", 0.01m);
}

static void HourlyTrainingEconomics()
{
    var definition = new TrainingSkillDefinition(
        "Hourly test",
        [
            new TrainingRateBand(
                0,
                100m,
                "Hourly method",
                new TrainingEconomics(
                    [
                        new TrainingResourceFlow(
                            1,
                            "Hourly supply",
                            0m,
                            TrainingFlowDirection.Input,
                            QuantityPerHour: 10m)
                    ],
                    FixedGpPerHour: 100m))
        ]);
    var prices = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100) };
    var calculator = new TrainingPlanCalculator();

    var baseline = calculator.Calculate(definition, 0, 100, prices);
    var doubled = calculator.Calculate(definition, 0, 100, prices, 200m);

    EqualDecimal(1m, baseline.Hours, "baseline hourly-method hours");
    EqualDecimal(-1_100m, baseline.NetGp ?? 0m, "baseline hourly-method cost");
    EqualDecimal(0.5m, doubled.Hours, "doubled hourly-method hours");
    EqualDecimal(-550m, doubled.NetGp ?? 0m, "doubled hourly-method cost");
}

static void TrainingMoneyMakerAllocation()
{
    var calculator = new TrainingMoneyMakingCalculator();
    var result = calculator.Calculate(2_400_000m, [10m, 2.5m, -4m]);
    EqualDecimal(12.5m, result.SelectedHours, "selected money-making hours");
    EqualDecimal(30_000_000m, result.NetGp, "selected money-making GP");

    var noMethod = calculator.Calculate(null, [12.5m]);
    EqualDecimal(12.5m, noMethod.SelectedHours, "selected hours remain visible without a method");
    EqualDecimal(0m, noMethod.NetGp, "no method contributes zero GP");
}

static TrainingRateBand TrainingBand(MainEhpCatalogue catalogue, string skill, long startExperience) =>
    catalogue.Skills.Single(definition => definition.Skill == skill)
        .Bands.Single(band => band.StartExperience == startExperience);

static TrainingResourceFlow Resource(TrainingRateBand band, int itemId) =>
    band.Economics?.Resources.Single(resource => resource.ItemId == itemId)
    ?? throw new InvalidOperationException($"{band.Method} is missing item {itemId}.");

static decimal TotalResourceQuantity(
    MainEhpCatalogue catalogue,
    string skill,
    int itemId,
    long startExperience = 0,
    long targetExperience = TrainingPlanCalculator.MaximumExperience)
{
    var definition = catalogue.Skills.Single(value => value.Skill == skill);
    var ordered = definition.Bands.OrderBy(band => band.StartExperience).ToArray();
    decimal total = 0m;
    for (var index = 0; index < ordered.Length; index++)
    {
        var band = ordered[index];
        var nextStart = index + 1 < ordered.Length
            ? ordered[index + 1].StartExperience
            : TrainingPlanCalculator.MaximumExperience;
        var segmentStart = Math.Max(startExperience, band.StartExperience);
        var segmentEnd = Math.Min(targetExperience, nextStart);
        if (segmentEnd <= segmentStart)
            continue;

        var resource = band.Economics?.Resources.SingleOrDefault(value => value.ItemId == itemId);
        if (resource is null)
            continue;
        var experience = segmentEnd - segmentStart;
        var hours = experience / band.ExperiencePerHour;
        total += resource.QuantityPerExperience * experience + resource.QuantityPerHour * hours;
    }

    return total;
}

static void XpPlannerPriceTooltips()
{
    var timestamp = new DateTimeOffset(2026, 7, 27, 5, 30, 0, TimeSpan.Zero);
    var prices = new Dictionary<int, ItemPrice>
    {
        [3002] = new ItemPrice(3002, 10_000, 9_000, timestamp, timestamp.AddMinutes(-1)),
        [6693] = new ItemPrice(6693, 5_000, 4_500, timestamp.AddMinutes(-2), timestamp.AddMinutes(-3)),
        [21163] = new ItemPrice(21163, 2_000, 1_800, timestamp.AddMinutes(-4), timestamp.AddMinutes(-5)),
        [6685] = new ItemPrice(6685, 12_000, 11_000, timestamp.AddMinutes(-6), timestamp.AddMinutes(-7))
    };
    var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Herblore");
    var row = new XpPlannerRowViewModel(
        definition,
        new TrainingPlanCalculator(),
        13_034_431,
        null,
        prices,
        () => { });

    Equal("Saradomin brews", row.Method, "active Herblore method");
    True(
        row.PriceToolTip.Contains(
            "Buy Toadflax potion (unf) @ 10,000 gp (high · 2026-07-27 05:30 UTC)",
            StringComparison.Ordinal),
        "base potion uses latest high");
    True(
        row.PriceToolTip.Contains(
            "Buy Crushed nest @ 5,000 gp (high · 2026-07-27 05:28 UTC)",
            StringComparison.Ordinal),
        "secondary ingredient uses latest high");
    True(
        row.PriceToolTip.Contains(
            "Buy Amulet of chemistry @ 2,000 gp (high · 2026-07-27 05:26 UTC)",
            StringComparison.Ordinal),
        "equipment charge input uses latest high");
    True(
        row.PriceToolTip.Contains(
            "Sell Saradomin brew(4) @ 11,000 gp (low · 2026-07-27 05:23 UTC)",
            StringComparison.Ordinal),
        "finished potion uses latest low");
    True(
        row.PriceToolTip.Contains("not guaranteed offers", StringComparison.OrdinalIgnoreCase),
        "tooltip discloses execution uncertainty");

    var fallbackPrices = new Dictionary<int, ItemPrice>(prices)
    {
        [6693] = new ItemPrice(6693, null, 4_500, null, timestamp.AddMinutes(-3))
    };
    row.UpdatePrices(fallbackPrices);
    True(
        row.PriceToolTip.Contains(
            "Buy Crushed nest @ 4,500 gp (low fallback · 2026-07-27 05:27 UTC)",
            StringComparison.Ordinal),
        "missing buy quote exposes low fallback");

    var outputFallback = TrainingMarketPricing.Select(
        TrainingFlowDirection.Output,
        new ItemPrice(6685, 12_000, null, timestamp, null));
    Equal(12_000L, outputFallback.UnitPrice ?? 0L, "output high fallback price");
    True(outputFallback.UsedFallbackPrice, "output high fallback state");

    var missingPrices = new Dictionary<int, ItemPrice>(prices);
    missingPrices.Remove(21163);
    row.UpdatePrices(missingPrices);
    True(
        row.PriceToolTip.Contains(
            "Buy Amulet of chemistry @ unavailable (no high or low quote)",
            StringComparison.Ordinal),
        "missing ingredient price is visible");
}

static async Task TrainingPlanPersistence()
{
    var directory = CreateTempDirectory();
    try
    {
        var path = Path.Combine(directory, "training-plans.json");
        var store = new JsonTrainingPlanStore(new TrainingPlanOptions { FilePath = path });
        await store.SaveAsync(
            "Player One",
            [new TrainingSkillPreference(
                "Construction",
                200_000_000,
                0,
                1_070_000,
                true,
                "mahogany-benches",
                new Dictionary<string, string>
                {
                    ["carpenters-outfit"] = bool.TrueString
                })]);
        await store.SaveAsync("Player Two", [new TrainingSkillPreference("Construction", 13_034_431)]);

        var first = await store.GetAsync(" player one ");
        var second = await store.GetAsync("PLAYER TWO");
        Equal(200_000_000L, first["Construction"].TargetExperience, "first profile goal");
        Equal(13_034_431L, second["Construction"].TargetExperience, "second profile goal");
        True(first["Construction"].StartExperienceOverride == 0, "explicit zero-XP override persists");
        True(first["Construction"].IsMoneyMakingSelected, "money-making skill allocation persists");
        Equal(
            "mahogany-benches",
            first["Construction"].TrainingMethodId ?? string.Empty,
            "training method selection persists");
        Equal(
            bool.TrueString,
            first["Construction"].Configuration!["carpenters-outfit"],
            "training configuration persists");
    }
    finally
    {
        Directory.Delete(directory, true);
    }
}

static async Task XpPlannerViewModelFlow()
{
    var client = new FakeHiscoreClient();
    var context = new CurrentProfileContext(
        client,
        new HiscoreParser(TimeProvider.System),
        new MemoryProfilePreferenceStore("bottleo"));
    var market = new FakeMarketDataService
    {
        Latest = new Dictionary<int, ItemPrice> { [8778] = Quote(8778, 431), [8782] = Quote(8782, 1_910) }
    };
    var viewModel = new XpPlannerViewModel(
        new MainEhpCatalogue(),
        new TrainingPlanCalculator(),
        new TrainingMoneyMakingCalculator(),
        market,
        new MemoryTrainingPlanStore(),
        context,
        new MoneyMakerSelectionContext());

    await viewModel.LoadAsync();
    Equal(21, viewModel.Rows.Count, "XP planner row count");
    True(
        viewModel.Rows.All(row => row.Skill is not "Attack" and not "Strength" and not "Hitpoints"),
        "XP planner omits zero-time melee and Hitpoints rows");
    Equal("bottleo", viewModel.ProfileName, "XP planner profile");
    var construction = viewModel.Rows.Single(row => row.Skill == "Construction");
    construction.StartExperience = 0;
    Equal("142.8", construction.Hours, "Construction displayed hours");
    True(construction.Result.NetGp is < -2_800_000_000m, "Construction live cost");
    True(construction.EconomicRate.EndsWith(" gp/hr"), "method subtitle identifies GP per hour");
    Equal(2, construction.AvailableMethods.Count, "Construction exposes default and oak-door routes");
    Equal("main-ehp", construction.SelectedMethodOption?.Id ?? string.Empty, "Construction defaults to Main EHP");
    construction.PersonalRate = 100_000m;
    True(construction.Hours != "142.8", "personal rate changes displayed hours");
    construction.ResetSkillCommand.Execute(null);
    Equal(construction.ProfileExperience, construction.StartExperience, "reset restores profile start XP");
    Equal(TrainingPlanCalculator.MaximumExperience, construction.TargetExperience, "reset restores 200m goal");
    EqualDecimal(
        construction.Result.BaseRate,
        construction.PersonalRate,
        "reset restores the selected method rate at profile XP");

    var moneyMakerSelection = new MoneyMakerSelectionContext();
    var allocatedViewModel = new XpPlannerViewModel(
        new MainEhpCatalogue(),
        new TrainingPlanCalculator(),
        new TrainingMoneyMakingCalculator(),
        market,
        new MemoryTrainingPlanStore(),
        context,
        moneyMakerSelection);
    await allocatedViewModel.LoadAsync();
    var allocatedConstruction = allocatedViewModel.Rows.Single(row => row.Skill == "Construction");
    allocatedConstruction.StartExperience = 0;
    allocatedConstruction.IsMoneyMakingSelected = true;
    moneyMakerSelection.Select("vyrewatch-sentinels", "Vyrewatch Sentinels", 2_400_000m, 3, false);
    EqualDecimal(
        allocatedConstruction.Result.Hours,
        allocatedViewModel.SelectedMoneyMakingHours,
        "only selected skill hours receive money-maker profit");
    EqualDecimal(
        allocatedConstruction.Result.Hours * 2_400_000m * 3m,
        allocatedViewModel.MoneyMakerGpContribution,
        "all-account money-maker contribution");
    var zeroTimeRanged = allocatedViewModel.Rows.Single(row => row.Skill == "Ranged");
    zeroTimeRanged.IsMoneyMakingSelected = true;
    EqualDecimal(
        allocatedConstruction.Result.Hours,
        allocatedViewModel.SelectedMoneyMakingHours,
        "zero-time selected skills do not add money-making hours");
    allocatedViewModel.ResetMoneyMakerCommand.Execute(null);
    True(moneyMakerSelection.Current is null, "XP Planner reset clears shared money maker");
    EqualDecimal(0m, allocatedViewModel.MoneyMakerGpContribution, "reset removes money-maker contribution");

    var slayer = viewModel.Rows.Single(row => row.Skill == "Slayer");
    var magic = viewModel.Rows.Single(row => row.Skill == "Magic");
    slayer.StartExperience = 0;
    slayer.TargetExperience = 6_578L * 28_397L;
    magic.StartExperience = 0;
    magic.TargetExperience = TrainingPlanCalculator.MaximumExperience;
    Equal(163_136_972L, magic.Result.AppliedExperienceCredit, "view-model Slayer credit");
    Equal("0", magic.Hours, "view-model zero-time Magic hours");
    True(magic.HasExperienceCredit, "view-model exposes pending Magic credit");
    True(magic.CreditSummary.Contains("163,136,972"), "view-model formats pending Magic credit");
}

static async Task ShellNavigation()
{
    var store = new MemoryFavouriteStore();
    var market = new FakeMarketDataService();
    var dashboard = new DashboardViewModel(store, market, [new VyrewatchMethod()]);
    var favourites = new FavouritesViewModel(store, market, TimeProvider.System);
    var moneyMakerSelection = new MoneyMakerSelectionContext();
    var money = new MoneyMakersViewModel(
        [new VyrewatchMethod()],
        new MoneyMakingCalculator(),
        market,
        new MemoryMoneyMakingPreferenceStore(),
        moneyMakerSelection);
    var profileContext = new CurrentProfileContext(
        new FakeHiscoreClient(),
        new HiscoreParser(TimeProvider.System),
        new MemoryProfilePreferenceStore("bottleo"));
    var profile = new ProfileViewModel(profileContext);
    var xpPlanner = new XpPlannerViewModel(
        new MainEhpCatalogue(),
        new TrainingPlanCalculator(),
        new TrainingMoneyMakingCalculator(),
        market,
        new MemoryTrainingPlanStore(),
        profileContext,
        moneyMakerSelection);
    var shell = new ShellViewModel(profile, dashboard, favourites, money, xpPlanner);

    await shell.InitializeAsync();
    Equal(PageKind.Dashboard, shell.CurrentPageKind, "startup page remains dashboard");
    await shell.NavigateCommand.ExecuteAsync("Favourites");

    Equal(PageKind.Favourites, shell.CurrentPageKind, "selected page");
    True(ReferenceEquals(favourites, shell.CurrentPage), "active page instance");

    await shell.NavigateCommand.ExecuteAsync("XpPlanner");

    Equal(PageKind.XpPlanner, shell.CurrentPageKind, "XP Planner selected page");
    True(ReferenceEquals(xpPlanner, shell.CurrentPage), "XP Planner active page instance");
    True(xpPlanner.Rows.Count > 0, "XP Planner rows loaded through shell navigation");
}

static async Task XpPlannerPriceFailure()
{
    var market = new FakeMarketDataService
    {
        Failure = new HttpRequestException("Market unavailable")
    };
    var profileContext = new CurrentProfileContext(
        new FakeHiscoreClient(),
        new HiscoreParser(TimeProvider.System),
        new MemoryProfilePreferenceStore("bottleo"));
    var viewModel = new XpPlannerViewModel(
        new MainEhpCatalogue(),
        new TrainingPlanCalculator(),
        new TrainingMoneyMakingCalculator(),
        market,
        new MemoryTrainingPlanStore(),
        profileContext,
        new MoneyMakerSelectionContext());

    await viewModel.LoadAsync();

    True(viewModel.Rows.Count > 0, "catalogue rows remain available");
    True(viewModel.ErrorMessage?.Contains("prices", StringComparison.OrdinalIgnoreCase) == true, "price warning is shown");
    True(viewModel.Rows.Any(row => row.TotalGp == "Not priced"), "affected economics remain visibly unpriced");
}

static Task WpfViewsConstruct()
{
    Exception? failure = null;
    var thread = new Thread(() =>
    {
        RunescapeTools.Wpf.App? application = null;
        try
        {
            application = new RunescapeTools.Wpf.App();
            application.InitializeComponent();
            var market = new FakeMarketDataService();
            var profileContext = new CurrentProfileContext(
                new FakeHiscoreClient(),
                new HiscoreParser(TimeProvider.System),
                new MemoryProfilePreferenceStore("bottleo"));
            var viewModel = new XpPlannerViewModel(
                new MainEhpCatalogue(),
                new TrainingPlanCalculator(),
                new TrainingMoneyMakingCalculator(),
                market,
                new MemoryTrainingPlanStore(),
                profileContext,
                new MoneyMakerSelectionContext());
            viewModel.LoadAsync().GetAwaiter().GetResult();
            var moneyViewModel = new MoneyMakersViewModel(
                [new VyrewatchMethod()],
                new MoneyMakingCalculator(),
                market,
                new MemoryMoneyMakingPreferenceStore(),
                new MoneyMakerSelectionContext());

            _ = new ProfileView();
            var plannerView = new XpPlannerView { DataContext = viewModel };
            var moneyView = new MoneyMakersView { DataContext = moneyViewModel };
            var favouritesView = new FavouritesView
            {
                DataContext = new FavouritesViewModel(
                    new MemoryFavouriteStore(),
                    market,
                    TimeProvider.System)
            };
            var window = new System.Windows.Window
            {
                Content = plannerView,
                Width = 1280,
                Height = 800,
                ShowInTaskbar = false,
                WindowStyle = System.Windows.WindowStyle.None
            };
            window.Show();
            plannerView.UpdateLayout();
            var farmingConfiguration = new MainEhpCatalogue().Skills
                .Single(skill => skill.Skill == "Farming")
                .Configurator!;
            var configurationDialog = new TrainingConfigurationDialog(
                new TrainingConfigurationDialogViewModel(
                    "Farming",
                    "Magic + dragonfruit tree runs",
                    farmingConfiguration.Definition,
                    farmingConfiguration.Definition.Normalize().Values,
                    "main-ehp"))
            {
                Owner = window
            };
            configurationDialog.Show();
            configurationDialog.UpdateLayout();
            configurationDialog.Close();
            window.Content = moneyView;
            moneyView.UpdateLayout();
            window.Content = favouritesView;
            favouritesView.UpdateLayout();
            window.Close();
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        finally
        {
            application?.Shutdown();
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();

    if (failure is not null)
        throw new InvalidOperationException("WPF view construction failed.", failure);

    return Task.CompletedTask;
}

static string HiscoreResponse(int skillLevel = 99)
{
    var rows = new List<string> { "123,2376,4567890123" };
    rows.AddRange(Enumerable.Range(0, OsrsHiscoreSkillOrder.Skills.Count)
        .Select(index => $"{1_000 + index},{skillLevel},{13_034_431L + index}"));
    rows.Add("-1,-1"); // Activity rows may use rank,score and are intentionally ignored.
    return string.Join('\n', rows);
}

static async Task ThrowsAsync<TException>(Func<Task> action, string label)
    where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"{label}: expected {typeof(TException).Name}");
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

sealed class FakeHiscoreClient : IHiscoreClient
{
    public Func<string, CancellationToken, Task<string>> Handler { get; set; }
        = (_, _) => Task.FromResult(CreateResponse());

    public Task<string> GetRawHiscoresAsync(string rsn, CancellationToken cancellationToken = default) =>
        Handler(rsn, cancellationToken);

    private static string CreateResponse()
    {
        var rows = new List<string> { "123,2376,4567890123" };
        rows.AddRange(Enumerable.Range(0, OsrsHiscoreSkillOrder.Skills.Count)
            .Select(index => $"{1_000 + index},99,{13_034_431L + index}"));
        return string.Join('\n', rows);
    }
}

sealed class MemoryProfilePreferenceStore(string selectedRsn) : IProfilePreferenceStore
{
    public string SelectedRsn { get; private set; } = selectedRsn;

    public Task<string> GetSelectedRsnAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(SelectedRsn);

    public Task SetSelectedRsnAsync(string rsn, CancellationToken cancellationToken = default)
    {
        SelectedRsn = rsn.Trim();
        return Task.CompletedTask;
    }
}

sealed class MemoryTrainingPlanStore : ITrainingPlanStore
{
    private readonly Dictionary<string, Dictionary<string, TrainingSkillPreference>> profiles =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyDictionary<string, TrainingSkillPreference>> GetAsync(
        string rsn,
        CancellationToken cancellationToken = default)
    {
        if (profiles.TryGetValue(rsn.Trim(), out var values))
            return Task.FromResult<IReadOnlyDictionary<string, TrainingSkillPreference>>(values);
        return Task.FromResult<IReadOnlyDictionary<string, TrainingSkillPreference>>(
            new Dictionary<string, TrainingSkillPreference>(StringComparer.OrdinalIgnoreCase));
    }

    public Task SaveAsync(
        string rsn,
        IReadOnlyCollection<TrainingSkillPreference> preferences,
        CancellationToken cancellationToken = default)
    {
        profiles[rsn.Trim()] = preferences.ToDictionary(value => value.Skill, StringComparer.OrdinalIgnoreCase);
        return Task.CompletedTask;
    }
}

sealed class MemoryMoneyMakingPreferenceStore : IMoneyMakingPreferenceStore
{
    public Dictionary<string, decimal> Overrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyDictionary<string, decimal>> GetActionsPerHourOverridesAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<string, decimal>>(
            new Dictionary<string, decimal>(Overrides, StringComparer.OrdinalIgnoreCase));

    public Task SetActionsPerHourOverrideAsync(
        string methodSlug,
        decimal? actionsPerHour,
        CancellationToken cancellationToken = default)
    {
        if (actionsPerHour.HasValue)
            Overrides[methodSlug] = actionsPerHour.Value;
        else
            Overrides.Remove(methodSlug);
        return Task.CompletedTask;
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
    public IReadOnlyDictionary<PriceTimeStep, IReadOnlyList<PricePoint>> HistoryByTimeStep { get; init; } =
        new Dictionary<PriceTimeStep, IReadOnlyList<PricePoint>>();
    public List<int> HistoryRequests { get; } = [];
    public List<PriceTimeStep> HistoryTimeSteps { get; } = [];
    public List<TimeSpan> HistoryWindows { get; } = [];
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

    public Task<IReadOnlyList<PricePoint>> GetHistoryAsync(
        int itemId,
        PriceTimeStep timeStep,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        HistoryRequests.Add(itemId);
        HistoryTimeSteps.Add(timeStep);
        HistoryWindows.Add(window);
        return Task.FromResult(
            HistoryByTimeStep.TryGetValue(timeStep, out var history)
                ? history
                : History);
    }

    public Task<IReadOnlyList<PricePoint>> GetWeeklyHistoryAsync(
        int itemId,
        CancellationToken cancellationToken = default) =>
        GetHistoryAsync(
            itemId,
            PriceTimeStep.OneHour,
            TimeSpan.FromDays(7),
            cancellationToken);
}

sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> responses = new(responses);
    public int Calls { get; private set; }
    public Uri? LastRequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastRequestUri = request.RequestUri;
        return Task.FromResult(responses.Dequeue());
    }
}
