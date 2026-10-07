using RunescapeTools.Wpf.Behaviors;

namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class FavouriteOrderingTests
{
    private static (FavouritesViewModel ViewModel, MemoryFavouriteStore Store,
        FakeMarketDataService Market, FakeItemIconService Icons) Create()
    {
        var now = DateTimeOffset.UtcNow;
        var store = new MemoryFavouriteStore(new FavouriteItem(1, "A", now),
            new FavouriteItem(2, "B", now), new FavouriteItem(3, "C", now));
        var market = new FakeMarketDataService
        {
            Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100), [2] = Quote(2, 200), [3] = Quote(3, 300) },
            History = [Point(now.AddHours(-1), 100)]
        };
        var icons = new FakeItemIconService(new ItemIcon(1, "A.png", @"C:\cache\a.png"));
        return (new FavouritesViewModel(store, market, icons, TimeProvider.System), store, market, icons);
    }

    [Test]
    public async Task ReorderPreservesRowsSelectionChartAndIconsWithoutNetworkRequests()
    {
        var (viewModel, store, market, icons) = Create();
        await viewModel.LoadAsync();
        var originalRows = viewModel.FavouriteRows.ToArray();
        var selection = viewModel.SelectedFavourite;
        viewModel.ZoomHistoryCommand.Execute(120);
        var zoom = viewModel.HistoryWindowLabel;
        var series = viewModel.ChartSeries;
        var latestRequests = market.LatestRequests.Count;
        var historyRequests = market.HistoryRequests.Count;
        var iconRequests = icons.RequestedItemIds.Count;

        await viewModel.ReorderFavouriteCommand.ExecuteAsync(new ListReorderRequest(originalRows[2], originalRows[0]));

        Assert.That(viewModel.FavouriteRows, Is.EqualTo(new[] { originalRows[2], originalRows[0], originalRows[1] }));
        Assert.That(viewModel.SelectedFavourite, Is.SameAs(selection));
        Assert.That(viewModel.ChartSeries, Is.SameAs(series));
        Assert.That(viewModel.HistoryWindowLabel, Is.EqualTo(zoom));
        Assert.That(originalRows[0].IconPath, Is.EqualTo(@"C:\cache\a.png"));
        Assert.That(market.LatestRequests, Has.Count.EqualTo(latestRequests));
        Assert.That(market.HistoryRequests, Has.Count.EqualTo(historyRequests));
        Assert.That(icons.RequestedItemIds, Has.Count.EqualTo(iconRequests));
        Assert.That((await store.GetAllAsync()).Select(item => item.ItemId), Is.EqualTo(new[] { 3, 1, 2 }));

        await viewModel.RefreshCommand.ExecuteAsync(null);
        Assert.That(viewModel.FavouriteRows.Select(row => row.ItemId), Is.EqualTo(new[] { 3, 1, 2 }));
        Assert.That(viewModel.SelectedFavourite!.ItemId, Is.EqualTo(1));
        var restored = new FavouritesViewModel(store, market, icons, TimeProvider.System);
        await restored.LoadAsync();
        Assert.That(restored.FavouriteRows.Select(row => row.ItemId), Is.EqualTo(new[] { 3, 1, 2 }));
        Assert.That(restored.SelectedFavourite!.ItemId, Is.EqualTo(3));
    }

    [Test]
    public async Task FailedMoveKeepsDisplayedAndSavedOrderThenAllowsRetry()
    {
        var (viewModel, store, _, _) = Create();
        await viewModel.LoadAsync();
        var original = viewModel.FavouriteRows.ToArray();
        var chart = viewModel.ChartSeries;
        store.MoveFailure = new IOException("Test save failure");
        var request = new ListReorderRequest(original[0], null);
        await viewModel.ReorderFavouriteCommand.ExecuteAsync(request);

        Assert.That(viewModel.FavouriteRows, Is.EqualTo(original));
        Assert.That(viewModel.SelectedFavourite, Is.SameAs(original[0]));
        Assert.That(viewModel.ChartSeries, Is.SameAs(chart));
        Assert.That((await store.GetAllAsync()).Select(item => item.ItemId), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(viewModel.ErrorMessage, Does.Contain("previous order has been kept"));
        Assert.That(viewModel.IsSavingOrder, Is.False);
        store.MoveFailure = null;
        await viewModel.ReorderFavouriteCommand.ExecuteAsync(request);
        Assert.That(viewModel.FavouriteRows.Select(row => row.ItemId), Is.EqualTo(new[] { 2, 3, 1 }));
        Assert.That(viewModel.ErrorMessage, Is.Null);
    }

    [Test]
    public async Task CancelledMoveKeepsThePreviousOrderAndReleasesBusyState()
    {
        var (viewModel, store, _, _) = Create();
        await viewModel.LoadAsync();
        store.MoveDelay = new TaskCompletionSource().Task;
        var original = viewModel.FavouriteRows.ToArray();
        var operation = viewModel.ReorderFavouriteCommand.ExecuteAsync(new ListReorderRequest(original[2], original[0]));
        Assert.That(viewModel.IsSavingOrder, Is.True);
        viewModel.ReorderFavouriteCommand.Cancel();
        await operation;

        Assert.That(viewModel.FavouriteRows, Is.EqualTo(original));
        Assert.That((await store.GetAllAsync()).Select(item => item.ItemId), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(viewModel.IsSavingOrder, Is.False);
        Assert.That(viewModel.ErrorMessage, Is.Null);
    }

    [Test]
    public async Task RefreshWaitsForSaveAndOtherMutationsAreDisabledDuringIt()
    {
        var (viewModel, store, market, _) = Create();
        await viewModel.LoadAsync();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        store.MoveDelay = release.Task;
        var request = new ListReorderRequest(viewModel.FavouriteRows[2], viewModel.FavouriteRows[0]);
        var operation = viewModel.ReorderFavouriteCommand.ExecuteAsync(request);
        Assert.That(viewModel.IsSavingOrder, Is.True);
        Assert.That(viewModel.ReorderFavouriteCommand.CanExecute(request), Is.False);
        Assert.That(viewModel.AddFavouriteCommand.CanExecute(new SearchResultRow(Map(4, "D"), "D", "Item 4")), Is.False);
        Assert.That(viewModel.RemoveFavouriteCommand.CanExecute(viewModel.FavouriteRows[0]), Is.False);
        Assert.That(viewModel.RefreshCommand.CanExecute(null), Is.False);
        var refresh = viewModel.LoadAsync();
        Assert.That(market.LatestRequests, Has.Count.EqualTo(1));
        release.SetResult();
        await Task.WhenAll(operation, refresh);

        Assert.That(viewModel.FavouriteRows.Select(row => row.ItemId), Is.EqualTo(new[] { 3, 1, 2 }));
        Assert.That(viewModel.IsSavingOrder, Is.False);
    }
}
