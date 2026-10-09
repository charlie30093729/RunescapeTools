using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Favourites;
using RunescapeTools.Core.MoneyMaking;

namespace RunescapeTools.Wpf.ViewModels;

public partial class DashboardPriceRow(
    string monogram,
    string name,
    int itemId,
    string price) : ObservableObject
{
    public string Monogram { get; } = monogram;
    public string Name { get; } = name;
    public int ItemId { get; } = itemId;
    public string Price { get; } = price;

    [ObservableProperty]
    private string? iconPath;
}

public partial class DashboardViewModel(
    IFavouriteStore favouriteStore,
    IMarketDataService marketData,
    IItemIconService itemIconService,
    IEnumerable<IMoneyMakingMethod> moneyMakingMethods) : ObservableObject, IPageViewModel
{
    private readonly IItemIconService itemIcons = itemIconService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private int favouriteCount;

    public int MethodCount { get; } = moneyMakingMethods.Count();
    public string HistoryWindow => "7d";
    public ObservableCollection<DashboardPriceRow> Prices { get; } = [];
    public bool HasPrices => Prices.Count > 0;

    public Task LoadAsync(CancellationToken cancellationToken = default) => RefreshAsync(cancellationToken);

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var favourites = await favouriteStore.GetAllAsync(cancellationToken);
            FavouriteCount = favourites.Count;
            var prices = await marketData.GetLatestForAsync(
                favourites.Select(item => item.ItemId),
                cancellationToken);

            Prices.Clear();
            foreach (var favourite in favourites)
            {
                prices.TryGetValue(favourite.ItemId, out var quote);
                Prices.Add(new DashboardPriceRow(
                    DisplayFormat.Monogram(favourite.Name),
                    favourite.Name,
                    favourite.ItemId,
                    DisplayFormat.Gp(quote?.MidPrice)));
            }

            OnPropertyChanged(nameof(HasPrices));
            await LoadFavouriteIconsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            ErrorMessage = "Live prices are temporarily unavailable. Your favourites are still safe.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadFavouriteIconsAsync(CancellationToken cancellationToken)
    {
        var rows = Prices.ToArray();
        if (rows.Length == 0)
            return;

        try
        {
            var icons = await itemIcons.GetManyAsync(
                rows.Select(row => row.ItemId),
                cancellationToken);
            foreach (var row in rows)
            {
                if (icons.TryGetValue(row.ItemId, out var icon))
                    row.IconPath = icon.LocalFilePath;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Icons are optional decoration; price data remains fully usable.
        }
    }
}
