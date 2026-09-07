namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class DashboardViewModelTests
{
    [Test]
    [Property("LegacyScenario", "DashboardViewModelStates")]
    [Description("dashboard view-model loads and reports failures")]
    public async Task DashboardViewModelStates()
    {
        var store = new MemoryFavouriteStore(new FavouriteItem(1, "Rune bar", DateTimeOffset.UtcNow));
        var market = new FakeMarketDataService { Latest = new Dictionary<int, ItemPrice> { [1] = Quote(1, 500) } };
        var iconPath = @"C:\cache\rune-bar.png";
        var viewModel = new DashboardViewModel(
            store,
            market,
            new FakeItemIconService(new ItemIcon(1, "Rune bar.png", iconPath)),
            [new VyrewatchMethod()]);

        await viewModel.LoadAsync();
        Assert.That(viewModel.FavouriteCount, Is.EqualTo(1), "dashboard favourite count");
        Assert.That(viewModel.Prices.Count, Is.EqualTo(1), "dashboard price rows");
        Assert.That(viewModel.Prices[0].IconPath ?? string.Empty, Is.EqualTo(iconPath), "dashboard favourite icon");

        market.Failure = new HttpRequestException("offline");
        await viewModel.LoadAsync();
        Assert.That(!string.IsNullOrWhiteSpace(viewModel.ErrorMessage), Is.True, "dashboard error state");
    }
}
