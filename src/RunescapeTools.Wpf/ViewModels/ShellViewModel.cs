using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RunescapeTools.Wpf.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IReadOnlyDictionary<PageKind, IPageViewModel> pages;
    private CancellationTokenSource? navigationCancellation;

    [ObservableProperty]
    private IPageViewModel? currentPage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProfileSelected))]
    [NotifyPropertyChangedFor(nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(nameof(IsFavouritesSelected))]
    [NotifyPropertyChangedFor(nameof(IsMoneyMakersSelected))]
    [NotifyPropertyChangedFor(nameof(IsXpPlannerSelected))]
    private PageKind currentPageKind;

    [ObservableProperty]
    private bool isNavigating;

    public ShellViewModel(
        ProfileViewModel profile,
        DashboardViewModel dashboard,
        FavouritesViewModel favourites,
        MoneyMakersViewModel moneyMakers,
        XpPlannerViewModel xpPlanner)
    {
        pages = new Dictionary<PageKind, IPageViewModel>
        {
            [PageKind.Profile] = profile,
            [PageKind.Dashboard] = dashboard,
            [PageKind.Favourites] = favourites,
            [PageKind.MoneyMakers] = moneyMakers,
            [PageKind.XpPlanner] = xpPlanner
        };
        CurrentPageKind = PageKind.Dashboard;
        CurrentPage = dashboard;
    }

    public bool IsProfileSelected => CurrentPageKind == PageKind.Profile;
    public bool IsDashboardSelected => CurrentPageKind == PageKind.Dashboard;
    public bool IsFavouritesSelected => CurrentPageKind == PageKind.Favourites;
    public bool IsMoneyMakersSelected => CurrentPageKind == PageKind.MoneyMakers;
    public bool IsXpPlannerSelected => CurrentPageKind == PageKind.XpPlanner;
    public string Today => DateTime.Now.ToString("dddd, d MMMM yyyy");

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        NavigateCoreAsync(PageKind.Dashboard, cancellationToken);

    [RelayCommand]
    private Task NavigateAsync(string? destination)
    {
        if (!Enum.TryParse<PageKind>(destination, true, out var page))
            return Task.CompletedTask;

        return NavigateCoreAsync(page, CancellationToken.None);
    }

    private async Task NavigateCoreAsync(PageKind page, CancellationToken cancellationToken)
    {
        navigationCancellation?.Cancel();
        navigationCancellation?.Dispose();
        navigationCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = navigationCancellation.Token;

        CurrentPageKind = page;
        CurrentPage = pages[page];
        IsNavigating = true;
        try
        {
            await CurrentPage.LoadAsync(token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        finally
        {
            if (!token.IsCancellationRequested)
                IsNavigating = false;
        }
    }
}
