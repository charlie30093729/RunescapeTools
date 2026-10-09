namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class FavouritesViewModelTests
{
    [Test]
    [Property("LegacyScenario", "FavouritesViewModelFlow")]
    [Description("favourites view-model searches, adds, selects, and removes")]
    public async Task FavouritesViewModelFlow()
    {
        var store = new MemoryFavouriteStore(new FavouriteItem(1, "Rune bar", DateTimeOffset.UtcNow));
        var market = new FakeMarketDataService
        {
            Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 500), [2] = Quote(2, 900) },
            SearchResults = [Map(2, "Rune platebody")],
            History = [Point(DateTimeOffset.UtcNow.AddDays(-1), 400), Point(DateTimeOffset.UtcNow, 500)]
        };
        var viewModel = new FavouritesViewModel(
            store,
            market,
            new FakeItemIconService(),
            TimeProvider.System);

        await viewModel.LoadAsync();
        var searchCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        System.ComponentModel.PropertyChangedEventHandler searchChanged = (_, args) =>
        {
            if (args.PropertyName == nameof(viewModel.HasSearchResults) && viewModel.HasSearchResults)
                searchCompleted.TrySetResult();
        };
        viewModel.PropertyChanged += searchChanged;
        try
        {
            viewModel.SearchText = "rune";
            await searchCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            viewModel.PropertyChanged -= searchChanged;
        }
        Assert.That(viewModel.SearchResults.Count, Is.EqualTo(1), "debounced search results");

        await viewModel.AddFavouriteCommand.ExecuteAsync(viewModel.SearchResults[0]);
        Assert.That(viewModel.FavouriteCount, Is.EqualTo(2), "favourite added");
        Assert.That(viewModel.SelectedFavourite?.ItemId ?? 0, Is.EqualTo(2), "new favourite selected");

        await viewModel.RemoveFavouriteCommand.ExecuteAsync(viewModel.SelectedFavourite);
        Assert.That(viewModel.FavouriteCount, Is.EqualTo(1), "favourite removed");
        Assert.That(viewModel.SelectedFavourite?.ItemId ?? 0, Is.EqualTo(1), "selection moved after removal");
    }

    [Test]
    [Property("LegacyScenario", "FavouritesItemIcons")]
    [Description("favourites load cached item icons without affecting fallback rows")]
    public async Task FavouritesItemIcons()
    {
        var store = new MemoryFavouriteStore(
            new FavouriteItem(1, "Rune bar", DateTimeOffset.UtcNow),
            new FavouriteItem(2, "Rune platebody", DateTimeOffset.UtcNow));
        var market = new FakeMarketDataService
        {
            Latest = new Dictionary<int, ItemPrice>
            {
                [1] = Quote(1, 500),
                [2] = Quote(2, 900)
            },
            History = [Point(DateTimeOffset.UtcNow.AddHours(-1), 500)]
        };
        var iconService = new FakeItemIconService(
            new ItemIcon(1, "Rune bar.png", @"C:\cache\rune-bar.png"));
        var viewModel = new FavouritesViewModel(
            store,
            market,
            iconService,
            TimeProvider.System);

        await viewModel.LoadAsync();

        Assert.That(viewModel.FavouriteRows.Single(row => row.ItemId == 1).IconPath ?? string.Empty, Is.EqualTo(@"C:\cache\rune-bar.png"), "resolved favourite icon path");
        Assert.That(viewModel.FavouriteRows.Single(row => row.ItemId == 2).IconPath is null, Is.True, "missing icon keeps the monogram fallback");
        Assert.That(viewModel.FavouriteRows.All(row => !string.IsNullOrWhiteSpace(row.Name)), Is.True, "missing icons do not remove favourite data");
        Assert.That(iconService.RequestedItemIds.Order().SequenceEqual([1, 2]), Is.True, "only persisted favourite IDs request icons");
    }

    [Test]
    [Property("LegacyScenario", "FavouritesChartZoomFlow")]
    [Description("favourites chart uses discrete one-day to one-month zoom")]
    public async Task FavouritesChartZoomFlow()
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
            new FakeItemIconService(),
            new TestTimeProvider(now));

        await viewModel.LoadAsync();

        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("7 DAYS"), "default history window");
        var series = (LineSeries<FavouritePriceChartPoint>)viewModel.ChartSeries.Single();
        Assert.That(series.Values?.Count() ?? 0, Is.EqualTo(3), "chart uses enriched price and volume points");
        viewModel.ZoomHistoryCommand.Execute(120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("3 DAYS"), "first zoom-in window");
        viewModel.ZoomHistoryCommand.Execute(120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("1 DAY"), "second zoom-in window");
        viewModel.ZoomHistoryCommand.Execute(120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("1 DAY"), "minimum history window");

        viewModel.ZoomHistoryCommand.Execute(-120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("3 DAYS"), "first zoom-out window");
        viewModel.ZoomHistoryCommand.Execute(-120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("7 DAYS"), "default zoom-out window");
        viewModel.ZoomHistoryCommand.Execute(-120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("1 MONTH"), "maximum history window");
        viewModel.ZoomHistoryCommand.Execute(-120);
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo("1 MONTH"), "clamped maximum history window");
        Assert.That(market.HistoryTimeSteps.SequenceEqual(
                [PriceTimeStep.SixHours, PriceTimeStep.OneHour]), Is.True, "favourites loads hourly and monthly resolutions");
        Assert.That(market.HistoryWindows.SequenceEqual(
                [TimeSpan.FromDays(31), TimeSpan.FromDays(8)]), Is.True, "favourites loads a one-day rolling-volume lookback");
    }

    [Test]
    [Property("LegacyScenario", "FavouritesChartVolume")]
    [Description("favourites chart points retain rolling 24-hour volume")]
    public void FavouritesChartVolume()
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

        Assert.That(current.RollingHighVolume, Is.EqualTo(4L), "rolling high-side volume");
        Assert.That(current.RollingLowVolume, Is.EqualTo(6L), "rolling low-side volume");
        Assert.That(current.RollingVolume, Is.EqualTo(10L), "rolling total volume");
        Assert.That(current.TooltipText.Contains("24h tracked volume: 10 items"), Is.True, "volume tooltip total");
        Assert.That(current.TooltipText.Contains("High side: 4"), Is.True, "volume tooltip high side");
        Assert.That(current.TooltipText.Contains("Low side: 6"), Is.True, "volume tooltip low side");
    }
}
