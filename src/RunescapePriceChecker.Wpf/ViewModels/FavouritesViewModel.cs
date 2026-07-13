using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using RunescapePriceChecker.Application.Market;
using RunescapePriceChecker.Core.Favourites;
using RunescapePriceChecker.Core.Market;
using SkiaSharp;

namespace RunescapePriceChecker.Wpf.ViewModels;

public sealed record FavouriteRow(
    FavouriteItem Favourite,
    string Monogram,
    string Price,
    string ItemNumber)
{
    public int ItemId => Favourite.ItemId;
    public string Name => Favourite.Name;
}

public sealed record SearchResultRow(ItemMapping Item, string Monogram, string ItemNumber)
{
    public int ItemId => Item.Id;
    public string Name => Item.Name;
}

public partial class FavouritesViewModel(
    IFavouriteStore favouriteStore,
    IMarketDataService marketData,
    TimeProvider timeProvider) : ObservableObject, IPageViewModel
{
    private readonly TimeProvider clock = timeProvider;
    private CancellationTokenSource? searchCancellation;
    private CancellationTokenSource? selectionCancellation;
    private bool suppressSelectionLoad;
    private IReadOnlyList<FavouriteItem> favourites = [];
    private IReadOnlyDictionary<int, ItemPrice> latest = new Dictionary<int, ItemPrice>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavouriteCountText))]
    private int favouriteCount;

    [ObservableProperty]
    private FavouriteRow? selectedFavourite;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isLoadingHistory;

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string currentMidpoint = "Unavailable";

    [ObservableProperty]
    private string instantBuy = "Unavailable";

    [ObservableProperty]
    private string instantSell = "Unavailable";

    [ObservableProperty]
    private string weeklyChange = "No weekly change";

    [ObservableProperty]
    private bool isWeeklyChangePositive = true;

    [ObservableProperty]
    private string weeklyPoints = "0";

    [ObservableProperty]
    private string trackedVolume = "0";

    [ObservableProperty]
    private IEnumerable<ISeries> chartSeries = Array.Empty<ISeries>();

    public ObservableCollection<FavouriteRow> FavouriteRows { get; } = [];
    public ObservableCollection<SearchResultRow> SearchResults { get; } = [];
    public string FavouriteCountText => $"{FavouriteCount} favourite{(FavouriteCount == 1 ? string.Empty : "s")}";
    public bool HasFavourites => FavouriteRows.Count > 0;
    public bool HasSearchResults => SearchResults.Count > 0;
    public IEnumerable<Axis> XAxes { get; } =
    [
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToLocalTime().ToString("ddd"))
        {
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(new SKColor(107, 100, 88)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(226, 217, 198)) { StrokeThickness = 1 }
        }
    ];
    public IEnumerable<Axis> YAxes { get; } =
    [
        new Axis
        {
            Labeler = value => $"{value / 1_000_000d:0.#}m",
            TextSize = 12,
            LabelsPaint = new SolidColorPaint(new SKColor(107, 100, 88)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(226, 217, 198)) { StrokeThickness = 1 }
        }
    ];

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var selectedId = SelectedFavourite?.ItemId;
            favourites = await favouriteStore.GetAllAsync(cancellationToken);
            latest = await marketData.GetLatestForAsync(
                favourites.Select(item => item.ItemId),
                cancellationToken);
            RebuildFavouriteRows(selectedId);

            if (SelectedFavourite is not null)
                await LoadSelectedHistoryAsync(SelectedFavourite, cancellationToken);
            else
                ResetQuote();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            ErrorMessage = "The live price service is unavailable right now. Try again in a moment.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        searchCancellation?.Cancel();
        searchCancellation?.Dispose();
        searchCancellation = new CancellationTokenSource();
        _ = SearchAfterDelayAsync(value, searchCancellation.Token);
    }

    partial void OnSelectedFavouriteChanged(FavouriteRow? value)
    {
        if (suppressSelectionLoad || value is null)
            return;

        selectionCancellation?.Cancel();
        selectionCancellation?.Dispose();
        selectionCancellation = new CancellationTokenSource();
        _ = LoadSelectedHistoryAsync(value, selectionCancellation.Token);
    }

    [RelayCommand]
    private async Task SelectFavouriteAsync(FavouriteRow? row)
    {
        if (row is null)
            return;

        SelectedFavourite = row;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddFavouriteAsync(SearchResultRow? row, CancellationToken cancellationToken)
    {
        if (row is null)
            return;

        var favourite = new FavouriteItem(row.ItemId, row.Name, clock.GetUtcNow());
        await favouriteStore.AddAsync(favourite, cancellationToken);
        SearchText = string.Empty;
        SearchResults.Clear();
        OnPropertyChanged(nameof(HasSearchResults));
        await ReloadAfterMutationAsync(favourite.ItemId, cancellationToken);
    }

    [RelayCommand]
    private async Task RemoveFavouriteAsync(FavouriteRow? row, CancellationToken cancellationToken)
    {
        if (row is null)
            return;

        var removedSelected = SelectedFavourite?.ItemId == row.ItemId;
        await favouriteStore.RemoveAsync(row.ItemId, cancellationToken);
        await ReloadAfterMutationAsync(removedSelected ? null : SelectedFavourite?.ItemId, cancellationToken);
    }

    [RelayCommand]
    private Task RefreshAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);

    private async Task ReloadAfterMutationAsync(int? selectedId, CancellationToken cancellationToken)
    {
        favourites = await favouriteStore.GetAllAsync(cancellationToken);
        latest = await marketData.GetLatestForAsync(
            favourites.Select(item => item.ItemId),
            cancellationToken);
        RebuildFavouriteRows(selectedId);
        if (SelectedFavourite is not null)
            await LoadSelectedHistoryAsync(SelectedFavourite, cancellationToken);
        else
            ResetQuote();
    }

    private void RebuildFavouriteRows(int? selectedId)
    {
        FavouriteRows.Clear();
        foreach (var favourite in favourites)
        {
            latest.TryGetValue(favourite.ItemId, out var price);
            FavouriteRows.Add(new FavouriteRow(
                favourite,
                DisplayFormat.Monogram(favourite.Name),
                DisplayFormat.Gp(price?.MidPrice),
                $"Item {favourite.ItemId}"));
        }

        FavouriteCount = FavouriteRows.Count;
        suppressSelectionLoad = true;
        try
        {
            SelectedFavourite = FavouriteRows.FirstOrDefault(row => row.ItemId == selectedId)
                                ?? FavouriteRows.FirstOrDefault();
        }
        finally
        {
            suppressSelectionLoad = false;
        }
        OnPropertyChanged(nameof(HasFavourites));
    }

    private async Task LoadSelectedHistoryAsync(FavouriteRow row, CancellationToken cancellationToken)
    {
        IsLoadingHistory = true;
        ErrorMessage = null;
        ChartSeries = Array.Empty<ISeries>();

        latest.TryGetValue(row.ItemId, out var price);
        CurrentMidpoint = DisplayFormat.Gp(price?.MidPrice);
        InstantBuy = DisplayFormat.Gp(price?.High);
        InstantSell = DisplayFormat.Gp(price?.Low);

        try
        {
            var history = await marketData.GetWeeklyHistoryAsync(row.ItemId, cancellationToken);
            SetHistory(history);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            ErrorMessage = "Weekly history could not be loaded from the Wiki price service.";
        }
        finally
        {
            IsLoadingHistory = false;
        }
    }

    private void SetHistory(IReadOnlyList<PricePoint> history)
    {
        var points = history
            .Where(point => point.MidPrice.HasValue)
            .Select(point => new DateTimePoint(
                point.Timestamp.UtcDateTime,
                (double?)point.MidPrice))
            .ToArray();

        WeeklyPoints = history.Count.ToString("N0");
        TrackedVolume = DisplayFormat.Compact(history.Sum(point => point.HighVolume + point.LowVolume));

        var startValue = points.FirstOrDefault()?.Value;
        var endValue = points.LastOrDefault()?.Value;
        if (startValue is not null and not 0 && endValue is not null)
        {
            var change = (endValue.Value - startValue.Value) / startValue.Value * 100d;
            IsWeeklyChangePositive = change >= 0;
            WeeklyChange = $"{(change >= 0 ? "+" : string.Empty)}{change:N1}% this week";
        }
        else
        {
            IsWeeklyChangePositive = true;
            WeeklyChange = "No weekly change";
        }

        ChartSeries =
        [
            new LineSeries<DateTimePoint>
            {
                Name = "Mid price",
                Values = points,
                LineSmoothness = 0.35,
                GeometrySize = 7,
                Stroke = new SolidColorPaint(new SKColor(158, 111, 33), 3),
                GeometryStroke = new SolidColorPaint(new SKColor(158, 111, 33), 2),
                GeometryFill = new SolidColorPaint(new SKColor(252, 248, 238)),
                Fill = new LinearGradientPaint(
                    [new SKColor(184, 132, 40, 90), new SKColor(184, 132, 40, 4)],
                    new SKPoint(0.5f, 0),
                    new SKPoint(0.5f, 1)),
                XToolTipLabelFormatter = chartPoint =>
                    chartPoint.Model?.DateTime.ToLocalTime().ToString("ddd d MMM, h:mm tt") ?? string.Empty,
                YToolTipLabelFormatter = chartPoint =>
                    chartPoint.Model?.Value is { } value ? $"{value:N0} gp" : "Unavailable"
            }
        ];
    }

    private async Task SearchAfterDelayAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(275), cancellationToken);
            if (query.Trim().Length < 2)
            {
                SearchResults.Clear();
                OnPropertyChanged(nameof(HasSearchResults));
                return;
            }

            IsSearching = true;
            var matches = await marketData.SearchItemsAsync(query, 8, cancellationToken);
            var favouriteIds = favourites.Select(item => item.ItemId).ToHashSet();
            SearchResults.Clear();
            foreach (var item in matches.Where(item => !favouriteIds.Contains(item.Id)))
            {
                SearchResults.Add(new SearchResultRow(
                    item,
                    DisplayFormat.Monogram(item.Name),
                    $"Item {item.Id}"));
            }

            OnPropertyChanged(nameof(HasSearchResults));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            ErrorMessage = "Item search is temporarily unavailable.";
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                IsSearching = false;
        }
    }

    private void ResetQuote()
    {
        CurrentMidpoint = "Unavailable";
        InstantBuy = "Unavailable";
        InstantSell = "Unavailable";
        WeeklyChange = "No weekly change";
        WeeklyPoints = "0";
        TrackedVolume = "0";
        ChartSeries = Array.Empty<ISeries>();
    }
}
