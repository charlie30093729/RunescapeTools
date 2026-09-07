namespace RunescapeTools.Tests.Wpf.Views;

[TestFixture]
[Category("Wpf")]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class WpfViewSmokeTests
{
    [Test]
    [Property("LegacyScenario", "WpfViewsConstruct")]
    [Description("WPF profile, Favourites, Money Makers, and XP Planner views construct successfully")]
    public void WpfViewsConstruct()
    {
        RunescapeTools.Wpf.App? application = null;
        try
        {
            application = new RunescapeTools.Wpf.App();
            application.InitializeComponent();
            application.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            var safeImage = new RunescapeTools.Wpf.Controls.SafeImage
            {
                Source = new System.Windows.Media.Imaging.WriteableBitmap(
                    1,
                    1,
                    96,
                    96,
                    System.Windows.Media.PixelFormats.Bgra32,
                    null)
            };
            Assert.That(safeImage.HasLoadedImage, Is.True, "valid image source hides its fallback");
            safeImage.Source = null;
            Assert.That(!safeImage.HasLoadedImage, Is.True, "cleared image source restores its fallback");
            var frozenImage = new System.Windows.Media.Imaging.WriteableBitmap(
                1,
                1,
                96,
                96,
                System.Windows.Media.PixelFormats.Bgra32,
                null);
            frozenImage.Freeze();
            safeImage.Source = frozenImage;
            Assert.That(safeImage.HasLoadedImage, Is.True, "frozen cached image is already loaded");
            safeImage.Source = null;
            Assert.That(!safeImage.HasLoadedImage, Is.True, "frozen cached image can be replaced safely");
            var market = new FakeMarketDataService
            {
                Latest = new Dictionary<int, ItemPrice>
                {
                    [24777] = Quote(24777, 500),
                    [24511] = Quote(24511, 900)
                }
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
            var favouritesViewModel = new FavouritesViewModel(
                new MemoryFavouriteStore(
                    new FavouriteItem(24777, "Blood shard", DateTimeOffset.UtcNow),
                    new FavouriteItem(24511, "Harmonised orb", DateTimeOffset.UtcNow)),
                market,
                new FakeItemIconService(),
                TimeProvider.System);
            favouritesViewModel.LoadAsync().GetAwaiter().GetResult();
            var favouritesView = new FavouritesView
            {
                DataContext = favouritesViewModel
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
            var skillRowsScrollViewer = (System.Windows.Controls.ScrollViewer)plannerView.FindName(
                "SkillRowsScrollViewer");
            Assert.That((decimal)skillRowsScrollViewer.VerticalOffset, Is.EqualTo(0m).Within(0m), "XP Planner skill rows start at the top");
            var skillRows = (System.Windows.Controls.ItemsControl)plannerView.FindName("SkillRows");
            var skillIconBar = (System.Windows.Controls.ItemsControl)plannerView.FindName("SkillIconBar");
            Assert.That(ReferenceEquals(
                    RunescapeTools.Wpf.Controls.RightClickItemNavigation.GetTargetItemsControl(skillIconBar),
                    skillRows), Is.True, "XP Planner icon bar attaches right-click navigation to the skill rows");
            Assert.That(RunescapeTools.Wpf.Controls.RightClickItemNavigation.ScrollToItem(
                    skillRows,
                    viewModel.Rows.Single(row => row.Skill == "Construction")), Is.True, "XP Planner can resolve a skill row from its icon data context");
            plannerView.UpdateLayout();
            Assert.That(skillRowsScrollViewer.VerticalOffset > 0, Is.True, "right-click skill navigation moves the planner scroll position");
            var runecraftConfiguration = new MainEhpCatalogue().Skills
                .Single(skill => skill.Skill == "Runecraft")
                .Configurator!;
            var configurationDialog = new TrainingConfigurationDialog(
                new TrainingConfigurationDialogViewModel(
                    "Runecraft",
                    "Solo lava runes",
                    runecraftConfiguration.Definition,
                    runecraftConfiguration.Definition.Normalize().Values,
                    "solo-lava-runes"))
            {
                Owner = window
            };
            configurationDialog.Show();
            configurationDialog.UpdateLayout();
            configurationDialog.Close();
            var constructionRow = viewModel.Rows.Single(row => row.Skill == "Construction");
            var priceDialog = new TrainingPriceDialog(
                new TrainingPriceDialogViewModel(
                    constructionRow.Skill,
                    constructionRow.Result,
                    market.Latest))
            {
                Owner = window
            };
            priceDialog.Show();
            priceDialog.UpdateLayout();
            priceDialog.Close();
            window.Content = moneyView;
            moneyView.UpdateLayout();
            window.Content = favouritesView;
            favouritesView.UpdateLayout();
            var favouritesList = (System.Windows.Controls.ListBox)favouritesView.FindName("FavouritesList");
            var selectedFavouriteName = (System.Windows.Controls.TextBlock)favouritesView.FindName("SelectedFavouriteName");
            var selectedFavouriteItemNumber = (System.Windows.Controls.TextBlock)favouritesView.FindName("SelectedFavouriteItemNumber");
            Assert.That(favouritesViewModel.SelectedFavourite?.ItemId ?? 0, Is.EqualTo(24777), "first favourite starts selected");
            Assert.That(selectedFavouriteName.Text, Is.EqualTo("Blood shard"), "initial favourite header name");
            favouritesList.SelectedIndex = 1;
            Assert.That(favouritesViewModel.SelectedFavourite?.ItemId ?? 0, Is.EqualTo(24511), "list selection reaches the view-model");
            Assert.That(selectedFavouriteName.Text, Is.EqualTo("Harmonised orb"), "selected favourite header name follows the list");
            Assert.That(selectedFavouriteItemNumber.Text, Is.EqualTo("Item 24511"), "selected favourite header ID follows the list");
            Assert.That(favouritesViewModel.CurrentMidpoint, Is.EqualTo("900 gp"), "selected favourite quote follows the list");
            window.Close();
        }
        finally
        {
            application?.Shutdown();
        }
    }
}
