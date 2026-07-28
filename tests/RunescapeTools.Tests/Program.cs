using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RunescapeTools.Application.Favourites;
using RunescapeTools.Application.Market;
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
    ("weekly history is filtered and cached", WeeklyHistoryIsFilteredAndCached),
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
    ("money-maker view-model shares and resets the priced selection", MoneyMakerViewModelFlow),
    ("profile view-model loads defaults and keeps valid data on errors", ProfileViewModelFlow),
    ("EHP catalogue covers every skill and ordered rate band", () => RunSync(EhpCatalogueCoverage)),
    ("training definitions support stable default and alternative methods", () => RunSync(TrainingMethodSelection)),
    ("XP Planner rows select and persist training methods", () => RunSync(XpPlannerRowMethodSelection)),
    ("approved deterministic methods expose reviewed rates and economics", () => RunSync(DeterministicMethodCatalogue)),
    ("Herblore brews include prescription goggles and alchemist amulet", () => RunSync(HerbloreEquipmentEconomics)),
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
    ("WPF profile, Money Makers, and XP Planner views construct successfully", WpfViewsConstruct)
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

static async Task MoneyMakerViewModelFlow()
{
    var method = new VyrewatchMethod();
    var secondMethod = new ZulrahMethod();
    var selection = new MoneyMakerSelectionContext();
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

    viewModel.UsingRegenPotions = false;
    Equal(9, viewModel.FlowRows.Count, "no-regen ledger removes the prayer regeneration potion");
    True(
        viewModel.FlowRows.All(row => row.Name != "Prayer regeneration potion(4)"),
        "no-regen ledger contains no prayer regeneration potion row");
    True(
        viewModel.MethodKicker.StartsWith("88 actions / hour", StringComparison.Ordinal),
        "no-regen selection uses 88 kills per hour");

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
    viewModel.SelectedMethod = primaryRow;
    Equal(method.Definition.Accounts + 1, viewModel.AccountCount, "account quantity is retained per method");
    viewModel.DecreaseAccountCountCommand.Execute(null);
    Equal(method.Definition.Accounts, viewModel.AccountCount, "account quantity decrements");

    selection.Clear();
    True(viewModel.SelectedMethod is null, "external reset clears Money Makers selection");
    Equal(0, viewModel.FlowRows.Count, "external reset clears displayed ledger");
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
        Equal(1, skill.AvailableMethods.Count, $"{skill.Skill} default method count");
        Equal("main-ehp", skill.AvailableMethods[0].Id, $"{skill.Skill} default method ID");
        Equal(skill.Bands.Count, skill.AvailableMethods[0].Bands.Count, $"{skill.Skill} default method bands");
    }
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

static void DeterministicMethodCatalogue()
{
    var catalogue = new MainEhpCatalogue();

    var prayer = TrainingBand(catalogue, "Prayer", 737_627);
    EqualDecimal(2_000_000m, prayer.ExperiencePerHour, "Prayer rate");
    Equal("Superior dragon bones at the Chaos Altar", prayer.Method, "Prayer method");
    EqualDecimal(1m / 1_050m, Resource(prayer, 22124).QuantityPerExperience, "Prayer bones per XP");

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
        "3002|6687|6693|21163",
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
        (1m + 0.15m / 3m) / 180m,
        Resource(herblore, 6687).QuantityPerExperience,
        "Alchemist's amulet brew output");
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
    EqualDecimal(623_700m, firemaking.ExperiencePerHour, "Firemaking rate");
    Equal("Rosewood logs - bow burning", firemaking.Method, "Firemaking method");
    EqualDecimal(1m / 420m, Resource(firemaking, 32910).QuantityPerExperience, "rosewood logs per XP");

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
        [6687] = new ItemPrice(6687, 600, 500, null, null)
    };

    var result = new TrainingPlanCalculator().Calculate(
        definition,
        2_192_818,
        2_642_818,
        prices);

    EqualDecimal(1m, result.Hours, "one hour of Saradomin brews");
    EqualDecimal(
        -5_826_250m,
        result.NetGp ?? 0m,
        "equipment-adjusted brew GP per hour",
        0.01m);
    EqualDecimal(
        -5_826_250m,
        result.AverageGpPerHour ?? 0m,
        "equipment-adjusted displayed GP per hour",
        0.01m);
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
    EqualDecimal(74m / 475m, Resource(runecraft75, 4698).QuantityPerExperience, "level-75 mud runes per XP");
    EqualDecimal(0.2m / 475m, Resource(runecraft75, 5521).QuantityPerExperience, "level-75 necklaces per XP");
    EqualDecimal(2.1m / 475m, Resource(runecraft75, 9075).QuantityPerExperience, "level-75 astrals per XP");

    var runecraft85 = TrainingBand(catalogue, "Runecraft", 3_258_594);
    EqualDecimal(96_900m, runecraft85.ExperiencePerHour, "Runecraft level-85 rate");
    EqualDecimal(63m / 598.5m, Resource(runecraft85, 7936).QuantityPerExperience, "level-85 essence per XP");
    EqualDecimal(93m / 598.5m, Resource(runecraft85, 4698).QuantityPerExperience, "level-85 mud runes per XP");
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
    Equal("Efficient tree runs", farming.Method, "Farming method");
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
    var prices = definition.Bands
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
        [6687] = new ItemPrice(6687, 12_000, 11_000, timestamp.AddMinutes(-6), timestamp.AddMinutes(-7))
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
            "Sell Saradomin brew(3) @ 11,000 gp (low · 2026-07-27 05:23 UTC)",
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
        new ItemPrice(6687, 12_000, null, timestamp, null));
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
                "mahogany-benches")]);
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
    Equal(1, construction.AvailableMethods.Count, "current catalogue exposes its default route");
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
                new MoneyMakerSelectionContext());

            _ = new ProfileView();
            var plannerView = new XpPlannerView { DataContext = viewModel };
            var moneyView = new MoneyMakersView { DataContext = moneyViewModel };
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
            window.Content = moneyView;
            moneyView.UpdateLayout();
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
    public Uri? LastRequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastRequestUri = request.RequestUri;
        return Task.FromResult(responses.Dequeue());
    }
}
