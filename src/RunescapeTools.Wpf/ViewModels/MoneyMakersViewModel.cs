using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.MoneyMaking;

namespace RunescapeTools.Wpf.ViewModels;

public sealed record MoneyMethodRow(IMoneyMakingMethod Method, string Index)
{
    public string Name => Method.Definition.Name;
    public string Actions => $"{Method.Definition.ActionsPerHour:N0} actions / hour";
}

public sealed record MoneyFlowRow(
    string Name,
    string ItemNumber,
    string Direction,
    bool IsOutput,
    string Quantity,
    string UnitPrice,
    string HourlyValue);

public partial class MoneyMakersViewModel : ObservableObject, IPageViewModel
{
    private readonly MoneyMakingCalculator calculator;
    private readonly IMarketDataService marketData;
    private readonly MoneyMakerSelectionContext selectionContext;
    private CancellationTokenSource? calculationCancellation;
    private bool initialized;
    private bool synchronizingSelection;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedMethod))]
    private MoneyMethodRow? selectedMethod;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string methodKicker = "NO METHOD SELECTED";

    [ObservableProperty]
    private string methodName = "Select a money maker";

    [ObservableProperty]
    private string methodDescription =
        "Choose a method from the list to price it and make it available to the XP Planner.";

    [ObservableProperty]
    private string profitAllAccounts = "Unavailable";

    [ObservableProperty]
    private bool isProfitPositive = true;

    [ObservableProperty]
    private string grossSales = "Unavailable";

    [ObservableProperty]
    private string tax = "Unavailable";

    [ObservableProperty]
    private string supplies = "Unavailable";

    [ObservableProperty]
    private string profitPerAccount = "Unavailable";

    [ObservableProperty]
    private string accountSummary = string.Empty;

    [ObservableProperty]
    private bool hasMissingPrices;

    public MoneyMakersViewModel(
        IEnumerable<IMoneyMakingMethod> methods,
        MoneyMakingCalculator calculator,
        IMarketDataService marketData,
        MoneyMakerSelectionContext selectionContext)
    {
        this.calculator = calculator;
        this.marketData = marketData;
        this.selectionContext = selectionContext;
        selectionContext.SelectionChanged += OnSharedSelectionChanged;
        var index = 1;
        foreach (var method in methods.OrderBy(method => method.Definition.Name))
            Methods.Add(new MoneyMethodRow(method, index++.ToString("00")));
    }

    public ObservableCollection<MoneyMethodRow> Methods { get; } = [];
    public ObservableCollection<MoneyFlowRow> FlowRows { get; } = [];
    public bool HasMethods => Methods.Count > 0;
    public bool HasSelectedMethod => SelectedMethod is not null;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!initialized)
        {
            var selectedSlug = selectionContext.Current?.Slug;
            synchronizingSelection = true;
            SelectedMethod = selectedSlug is null
                ? null
                : Methods.FirstOrDefault(row =>
                    row.Method.Definition.Slug.Equals(selectedSlug, StringComparison.OrdinalIgnoreCase));
            synchronizingSelection = false;
            initialized = true;
        }

        if (SelectedMethod is not null)
            await PriceMethodAsync(SelectedMethod, cancellationToken);
    }

    partial void OnSelectedMethodChanged(MoneyMethodRow? value)
    {
        if (synchronizingSelection || !initialized)
            return;

        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
        if (value is null)
        {
            selectionContext.Clear();
            ResetMethodDisplay();
            return;
        }

        if (!value.Method.Definition.Slug.Equals(
                selectionContext.Current?.Slug,
                StringComparison.OrdinalIgnoreCase))
        {
            synchronizingSelection = true;
            selectionContext.Clear();
            synchronizingSelection = false;
        }

        calculationCancellation = new CancellationTokenSource();
        _ = PriceMethodAsync(value, calculationCancellation.Token);
    }

    [RelayCommand]
    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (SelectedMethod is not null)
            await PriceMethodAsync(SelectedMethod, cancellationToken);
    }

    private async Task PriceMethodAsync(MoneyMethodRow selected, CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;
        FlowRows.Clear();
        try
        {
            var definition = selected.Method.Definition;
            var prices = await marketData.GetLatestForAsync(definition.RequiredItemIds, cancellationToken);
            var result = calculator.Calculate(definition, prices);

            MethodKicker = $"{result.Method.ActionsPerHour:N0} actions / hour · {result.Method.Accounts} accounts";
            MethodName = result.Method.Name;
            MethodDescription = result.Method.Description;
            ProfitAllAccounts = DisplayFormat.Gp(result.ProfitAllAccounts);
            IsProfitPositive = result.ProfitAllAccounts >= 0;
            GrossSales = DisplayFormat.Gp(result.GrossRevenuePerAccount);
            Tax = $"− {DisplayFormat.Gp(result.TaxPerAccount)}";
            Supplies = $"− {DisplayFormat.Gp(result.InputCostPerAccount)}";
            ProfitPerAccount = DisplayFormat.Gp(result.ProfitPerAccount);
            AccountSummary = $"across {result.Method.Accounts} accounts";
            HasMissingPrices = result.HasMissingPrices;
            selectionContext.Select(
                result.Method.Slug,
                result.Method.Name,
                result.ProfitPerAccount,
                result.HasMissingPrices);

            foreach (var line in result.Lines.OrderBy(line => line.Item.Direction))
            {
                var prefix = line.Item.Direction == ItemFlowDirection.Input ? "− " : "+ ";
                FlowRows.Add(new MoneyFlowRow(
                    line.Item.Name,
                    $"Item {line.Item.ItemId}",
                    line.Item.Direction.ToString(),
                    line.Item.Direction == ItemFlowDirection.Output,
                    DisplayFormat.Quantity(line.QuantityPerHour),
                    DisplayFormat.Gp(line.UnitPrice),
                    prefix + DisplayFormat.Gp(line.GrossValuePerHour)));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            ErrorMessage = "The method could not be priced because the Wiki market service is unavailable.";
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                IsLoading = false;
        }
    }

    private void OnSharedSelectionChanged(object? sender, EventArgs eventArgs)
    {
        if (synchronizingSelection || selectionContext.Current is not null)
            return;

        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
        calculationCancellation = null;
        synchronizingSelection = true;
        SelectedMethod = null;
        synchronizingSelection = false;
        ResetMethodDisplay();
    }

    private void ResetMethodDisplay()
    {
        MethodKicker = "NO METHOD SELECTED";
        MethodName = "Select a money maker";
        MethodDescription =
            "Choose a method from the list to price it and make it available to the XP Planner.";
        ProfitAllAccounts = "Unavailable";
        IsProfitPositive = true;
        GrossSales = "Unavailable";
        Tax = "Unavailable";
        Supplies = "Unavailable";
        ProfitPerAccount = "Unavailable";
        AccountSummary = string.Empty;
        HasMissingPrices = false;
        ErrorMessage = null;
        FlowRows.Clear();
    }
}
