using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapePriceChecker.Application.Market;
using RunescapePriceChecker.Core.MoneyMaking;

namespace RunescapePriceChecker.Wpf.ViewModels;

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
    private CancellationTokenSource? calculationCancellation;
    private bool initialized;

    [ObservableProperty]
    private MoneyMethodRow? selectedMethod;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string methodKicker = string.Empty;

    [ObservableProperty]
    private string methodName = string.Empty;

    [ObservableProperty]
    private string methodDescription = string.Empty;

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
        IMarketDataService marketData)
    {
        this.calculator = calculator;
        this.marketData = marketData;
        var index = 1;
        foreach (var method in methods.OrderBy(method => method.Definition.Name))
            Methods.Add(new MoneyMethodRow(method, index++.ToString("00")));
    }

    public ObservableCollection<MoneyMethodRow> Methods { get; } = [];
    public ObservableCollection<MoneyFlowRow> FlowRows { get; } = [];
    public bool HasMethods => Methods.Count > 0;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!initialized)
        {
            SelectedMethod = Methods.FirstOrDefault();
            initialized = true;
        }

        if (SelectedMethod is not null)
            await PriceMethodAsync(SelectedMethod, cancellationToken);
    }

    partial void OnSelectedMethodChanged(MoneyMethodRow? value)
    {
        if (!initialized || value is null)
            return;

        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
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
}
