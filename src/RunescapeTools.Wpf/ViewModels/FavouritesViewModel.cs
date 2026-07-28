using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Favourites;
using RunescapeTools.Core.Market;
using SkiaSharp;

namespace RunescapeTools.Wpf.ViewModels;

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
    private const int DefaultHistoryWindowIndex = 2;
    private static readonly HistoryWindowDefinition[] HistoryWindows =
    [
        new("1 DAY", TimeSpan.FromDays(1), PriceTimeStep.OneHour, TimeSpan.FromHours(4), "h tt", 6),
        new("3 DAYS", TimeSpan.FromDays(3), PriceTimeStep.OneHour, TimeSpan.FromHours(12), "ddd h tt", 5),
        new("7 DAYS", TimeSpan.FromDays(7), PriceTimeStep.OneHour, TimeSpan.FromDays(1), "ddd", 4),
        new("1 MONTH", TimeSpan.FromDays(30), PriceTimeStep.SixHours, TimeSpan.FromDays(5), "d MMM", 2)
    ];

    private readonly TimeProvider clock = timeProvider;
    private CancellationTokenSource? searchCancellation;
    private CancellationTokenSource? selectionCancellation;
    private bool suppressSelectionLoad;
    private bool monthlyHistoryLoadFailed;
    private int historyWindowIndex = DefaultHistoryWindowIndex;
    private IReadOnlyList<FavouriteItem> favourites = [];
    private IReadOnlyDictionary<int, ItemPrice> latest = new Dictionary<int, ItemPrice>();
    private IReadOnlyList<PricePoint> hourlyHistory = [];
    private IReadOnlyList<PricePoint>? monthlyHistory;

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HistoryAutomationName))]
    private string historyWindowLabel = HistoryWindows[DefaultHistoryWindowIndex].Label;

    [ObservableProperty]
    private IEnumerable<Axis> xAxes =
        CreateXAxis(HistoryWindows[DefaultHistoryWindowIndex], DateTimeOffset.UtcNow);

    public ObservableCollection<FavouriteRow> FavouriteRows { get; } = [];
    public ObservableCollection<SearchResultRow> SearchResults { get; } = [];
    public string FavouriteCountText => $"{FavouriteCount} favourite{(FavouriteCount == 1 ? string.Empty : "s")}";
    public bool HasFavourites => FavouriteRows.Count > 0;
    public bool HasSearchResults => SearchResults.Count > 0;
    public string HistoryAutomationName => $"{HistoryWindowLabel} midpoint price history";
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

    [RelayCommand]
    private void ZoomHistory(int wheelDelta)
    {
        if (wheelDelta == 0 || hourlyHistory.Count == 0)
            return;

        var direction = wheelDelta > 0 ? -1 : 1;
        var targetIndex = Math.Clamp(
            historyWindowIndex + direction,
            0,
            HistoryWindows.Length - 1);
        if (targetIndex == historyWindowIndex)
            return;

        if (HistoryWindows[targetIndex].TimeStep == PriceTimeStep.SixHours
            && monthlyHistory is null)
        {
            if (monthlyHistoryLoadFailed)
                ErrorMessage = "One-month history could not be loaded from the Wiki price service.";
            return;
        }

        historyWindowIndex = targetIndex;
        ApplyHistoryWindow();
    }

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
        ResetHistoryWindow();

        latest.TryGetValue(row.ItemId, out var price);
        CurrentMidpoint = DisplayFormat.Gp(price?.MidPrice);
        InstantBuy = DisplayFormat.Gp(price?.High);
        InstantSell = DisplayFormat.Gp(price?.Low);

        var monthlyTask = marketData.GetHistoryAsync(
            row.ItemId,
            PriceTimeStep.SixHours,
            TimeSpan.FromDays(30),
            cancellationToken);

        try
        {
            hourlyHistory = await marketData.GetHistoryAsync(
                row.ItemId,
                PriceTimeStep.OneHour,
                TimeSpan.FromDays(7),
                cancellationToken);
            SetWeeklyMetrics(hourlyHistory);
            ApplyHistoryWindow();
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

        try
        {
            monthlyHistory = await monthlyTask;
            monthlyHistoryLoadFailed = false;
            if (HistoryWindows[historyWindowIndex].TimeStep == PriceTimeStep.SixHours)
                ApplyHistoryWindow();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            monthlyHistoryLoadFailed = true;
        }
    }

    private void SetWeeklyMetrics(IReadOnlyList<PricePoint> history)
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
    }

    private void ApplyHistoryWindow()
    {
        var definition = HistoryWindows[historyWindowIndex];
        var history = definition.TimeStep == PriceTimeStep.SixHours
            ? monthlyHistory ?? []
            : hourlyHistory;
        var now = clock.GetUtcNow();
        var cutoff = now - definition.Duration;
        var points = history
            .Where(point => point.Timestamp >= cutoff
                            && point.Timestamp <= now
                            && point.MidPrice.HasValue)
            .Select(point => new DateTimePoint(
                point.Timestamp.UtcDateTime,
                (double?)point.MidPrice))
            .ToArray();

        HistoryWindowLabel = definition.Label;
        XAxes = CreateXAxis(definition, now);
        ChartSeries =
        [
            new LineSeries<DateTimePoint>
            {
                Name = "Mid price",
                Values = points,
                LineSmoothness = 0.35,
                GeometrySize = definition.GeometrySize,
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

    private void ResetHistoryWindow()
    {
        historyWindowIndex = DefaultHistoryWindowIndex;
        hourlyHistory = [];
        monthlyHistory = null;
        monthlyHistoryLoadFailed = false;
        var definition = HistoryWindows[historyWindowIndex];
        HistoryWindowLabel = definition.Label;
        XAxes = CreateXAxis(definition, clock.GetUtcNow());
    }

    private static IEnumerable<Axis> CreateXAxis(
        HistoryWindowDefinition definition,
        DateTimeOffset now)
    {
        var cutoff = now - definition.Duration;
        return
        [
            new DateTimeAxis(
                definition.AxisStep,
                date => date.ToLocalTime().ToString(definition.AxisLabelFormat))
            {
                MinLimit = cutoff.UtcDateTime.Ticks,
                MaxLimit = now.UtcDateTime.Ticks,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(new SKColor(107, 100, 88)),
                SeparatorsPaint = new SolidColorPaint(new SKColor(226, 217, 198))
                {
                    StrokeThickness = 1
                }
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
        ResetHistoryWindow();
    }

    private sealed record HistoryWindowDefinition(
        string Label,
        TimeSpan Duration,
        PriceTimeStep TimeStep,
        TimeSpan AxisStep,
        string AxisLabelFormat,
        double GeometrySize);
}
