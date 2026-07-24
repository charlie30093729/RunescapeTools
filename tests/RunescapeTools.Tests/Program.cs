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
    ("hiscore parser maps every current OSRS skill in API order", HiscoreParserMapsSkills),
    ("profile skill icons map to official Wiki assets", ProfileSkillIconMapping),
    ("hiscore parser rejects incomplete and malformed skill rows", HiscoreParserRejectsInvalidResponses),
    ("hiscore client URL-encodes RSNs and distinguishes missing accounts", HiscoreClientProtocol),
    ("profile preference seeds bottleo and persists successful selections", ProfilePreferencePersistence),
    ("profile context preserves valid state on failure and publishes refreshes", ProfileContextStateFlow),
    ("dashboard view-model loads and reports failures", DashboardViewModelStates),
    ("favourites view-model searches, adds, selects, and removes", FavouritesViewModelFlow),
    ("money-maker view-model reprices the complete ledger", MoneyMakerViewModelFlow),
    ("profile view-model loads defaults and keeps valid data on errors", ProfileViewModelFlow),
    ("EHP catalogue covers every skill and ordered rate band", () => RunSync(EhpCatalogueCoverage)),
    ("approved deterministic methods expose reviewed rates and economics", () => RunSync(DeterministicMethodCatalogue)),
    ("Construction route reproduces Main EHP hours and live-price economics", () => RunSync(ConstructionTrainingCalculation)),
    ("training rate overrides scale hours without changing total resources", () => RunSync(TrainingRateOverride)),
    ("hourly training costs respond to personal rate overrides", () => RunSync(HourlyTrainingEconomics)),
    ("training plans persist independently per RSN", TrainingPlanPersistence),
    ("XP planner loads profile goals and construction pricing", XpPlannerViewModelFlow),
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
    Equal(24, catalogue.Skills.Count, "catalogue skill count");
    Equal(
        string.Join('|', OsrsHiscoreSkillOrder.Skills),
        string.Join('|', catalogue.Skills.Select(skill => skill.Skill)),
        "catalogue skill order");

    foreach (var skill in catalogue.Skills)
    {
        True(skill.Bands.Count > 0, $"{skill.Skill} should have at least one rate band");
        True(skill.Bands.All(band => band.ExperiencePerHour > 0), $"{skill.Skill} rates should be positive");
        var ordered = skill.Bands.OrderBy(band => band.StartExperience).ToArray();
        Equal(ordered.Length, ordered.Select(band => band.StartExperience).Distinct().Count(), $"{skill.Skill} band starts");
        Equal(string.Join('|', ordered.Select(band => band.StartExperience)), string.Join('|', skill.Bands.Select(band => band.StartExperience)), $"{skill.Skill} band ordering");
    }
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
        "3002|6687|6693",
        string.Join('|', herblore.Economics!.Resources.Select(resource => resource.ItemId).Order()),
        "Herblore item IDs");

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

static TrainingRateBand TrainingBand(MainEhpCatalogue catalogue, string skill, long startExperience) =>
    catalogue.Skills.Single(definition => definition.Skill == skill)
        .Bands.Single(band => band.StartExperience == startExperience);

static TrainingResourceFlow Resource(TrainingRateBand band, int itemId) =>
    band.Economics?.Resources.Single(resource => resource.ItemId == itemId)
    ?? throw new InvalidOperationException($"{band.Method} is missing item {itemId}.");

static async Task TrainingPlanPersistence()
{
    var directory = CreateTempDirectory();
    try
    {
        var path = Path.Combine(directory, "training-plans.json");
        var store = new JsonTrainingPlanStore(new TrainingPlanOptions { FilePath = path });
        await store.SaveAsync("Player One", [new TrainingSkillPreference("Construction", 200_000_000, 0, 1_070_000)]);
        await store.SaveAsync("Player Two", [new TrainingSkillPreference("Construction", 13_034_431)]);

        var first = await store.GetAsync(" player one ");
        var second = await store.GetAsync("PLAYER TWO");
        Equal(200_000_000L, first["Construction"].TargetExperience, "first profile goal");
        Equal(13_034_431L, second["Construction"].TargetExperience, "second profile goal");
        True(first["Construction"].StartExperienceOverride == 0, "explicit zero-XP override persists");
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
        market,
        new MemoryTrainingPlanStore(),
        context);

    await viewModel.LoadAsync();
    Equal(24, viewModel.Rows.Count, "XP planner row count");
    Equal("bottleo", viewModel.ProfileName, "XP planner profile");
    var construction = viewModel.Rows.Single(row => row.Skill == "Construction");
    construction.StartExperience = 0;
    Equal("142.8", construction.Hours, "Construction displayed hours");
    True(construction.Result.NetGp is < -2_800_000_000m, "Construction live cost");
    construction.PersonalRate = 100_000m;
    True(construction.Hours != "142.8", "personal rate changes displayed hours");
    construction.ResetRateCommand.Execute(null);
    EqualDecimal(54_700m, construction.PersonalRate, "reset current method rate");
    Equal("142.8", construction.Hours, "reset restores catalogue hours");
}

static async Task ShellNavigation()
{
    var store = new MemoryFavouriteStore();
    var market = new FakeMarketDataService();
    var dashboard = new DashboardViewModel(store, market, [new VyrewatchMethod()]);
    var favourites = new FavouritesViewModel(store, market, TimeProvider.System);
    var money = new MoneyMakersViewModel([new VyrewatchMethod()], new MoneyMakingCalculator(), market);
    var profileContext = new CurrentProfileContext(
        new FakeHiscoreClient(),
        new HiscoreParser(TimeProvider.System),
        new MemoryProfilePreferenceStore("bottleo"));
    var profile = new ProfileViewModel(profileContext);
    var xpPlanner = new XpPlannerViewModel(
        new MainEhpCatalogue(),
        new TrainingPlanCalculator(),
        market,
        new MemoryTrainingPlanStore(),
        profileContext);
    var shell = new ShellViewModel(profile, dashboard, favourites, money, xpPlanner);

    await shell.InitializeAsync();
    Equal(PageKind.Dashboard, shell.CurrentPageKind, "startup page remains dashboard");
    await shell.NavigateCommand.ExecuteAsync("Favourites");

    Equal(PageKind.Favourites, shell.CurrentPageKind, "selected page");
    True(ReferenceEquals(favourites, shell.CurrentPage), "active page instance");
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
