using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Application.Market;
using RunescapeTools.Application.MoneyMaking;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.MoneyMaking;
using RunescapeTools.Core.MoneyMaking.Methods;

namespace RunescapeTools.Wpf.ViewModels;

public sealed record MoneyMethodRow(IMoneyMakingMethod Method, string Index)
{
    public string Name => Method.Definition.Name;
    public string Actions => $"{Method.Definition.ActionsPerHour:#,##0.##} default actions / hour";
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
    private readonly IMoneyMakingPreferenceStore preferenceStore;
    private readonly MoneyMakerSelectionContext selectionContext;
    private readonly Dictionary<string, int> accountCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, decimal> actionRateOverrides =
        new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? calculationCancellation;
    private IReadOnlyDictionary<int, ItemPrice>? currentPrices;
    private bool initialized;
    private bool synchronizingAccountCount;
    private bool synchronizingActionsPerHour;
    private bool synchronizingMethodOptions;
    private bool synchronizingSelection;
    private decimal lastValidActionsPerHour = 1m;

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
    private int accountCount = 1;

    [ObservableProperty]
    private bool hasMissingPrices;

    [ObservableProperty]
    private bool usingRegenPotions = true;

    [ObservableProperty]
    private bool pickingUpFrostDragonBones = true;

    [ObservableProperty]
    private decimal actionsPerHour = 1m;

    [ObservableProperty]
    private string defaultActionsPerHourText = string.Empty;

    [ObservableProperty]
    private bool isActionsPerHourOverridden;

    public MoneyMakersViewModel(
        IEnumerable<IMoneyMakingMethod> methods,
        MoneyMakingCalculator calculator,
        IMarketDataService marketData,
        IMoneyMakingPreferenceStore preferenceStore,
        MoneyMakerSelectionContext selectionContext)
    {
        this.calculator = calculator;
        this.marketData = marketData;
        this.preferenceStore = preferenceStore;
        this.selectionContext = selectionContext;
        selectionContext.SelectionChanged += OnSharedSelectionChanged;
        var index = 1;
        foreach (var method in methods
                     .OrderBy(GetDisplayPriority)
                     .ThenBy(method => method.Definition.Name, StringComparer.OrdinalIgnoreCase))
        {
            Methods.Add(new MoneyMethodRow(method, index++.ToString("00")));
            accountCounts[method.Definition.Slug] = Math.Max(1, method.Definition.Accounts);
        }
    }

    public ObservableCollection<MoneyMethodRow> Methods { get; } = [];
    public ObservableCollection<MoneyFlowRow> FlowRows { get; } = [];
    public bool HasMethods => Methods.Count > 0;
    public bool HasSelectedMethod => SelectedMethod is not null;
    public bool ShowRegenPotionOption => SelectedMethod?.Method is VyrewatchMethod;
    public bool ShowFrostDragonBonesOption => SelectedMethod?.Method is FrostDragonMethod;
    public bool CanDecreaseAccountCount => HasSelectedMethod && AccountCount > 1;
    public bool CanIncreaseAccountCount => HasSelectedMethod && AccountCount < int.MaxValue;

    private static int GetDisplayPriority(IMoneyMakingMethod method) =>
        method.Definition.Slug switch
        {
            "vyrewatch-sentinels" => 0,
            "zulrah" => 1,
            _ => 2
        };

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!initialized)
        {
            try
            {
                var persistedOverrides =
                    await preferenceStore.GetActionsPerHourOverridesAsync(cancellationToken);
                actionRateOverrides.Clear();
                foreach (var pair in persistedOverrides.Where(pair => pair.Value > 0m))
                    actionRateOverrides[pair.Key] = pair.Value;

                var frostOptions = await preferenceStore.GetBooleanOptionsAsync(
                    FrostDragonMethod.Slug,
                    cancellationToken);
                if (frostOptions.TryGetValue(
                        FrostDragonMethod.PickUpBonesOptionKey,
                        out var pickUpBones))
                {
                    synchronizingMethodOptions = true;
                    PickingUpFrostDragonBones = pickUpBones;
                    synchronizingMethodOptions = false;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                ErrorMessage =
                    "Saved action-rate settings could not be loaded; method defaults are being used.";
            }

            var existingSelection = selectionContext.Current;
            var selectedSlug = existingSelection?.Slug;
            synchronizingSelection = true;
            SelectedMethod = selectedSlug is null
                ? null
                : Methods.FirstOrDefault(row =>
                    row.Method.Definition.Slug.Equals(selectedSlug, StringComparison.OrdinalIgnoreCase));
            synchronizingSelection = false;
            if (SelectedMethod is not null && existingSelection is not null)
            {
                accountCounts[SelectedMethod.Method.Definition.Slug] = existingSelection.AccountCount;
                SetAccountCount(existingSelection.AccountCount);
                ConfigureActionsPerHour(SelectedMethod);
            }
            initialized = true;
        }

        if (SelectedMethod is not null)
            await PriceMethodAsync(SelectedMethod, cancellationToken);
    }

    partial void OnSelectedMethodChanged(MoneyMethodRow? value)
    {
        OnPropertyChanged(nameof(ShowRegenPotionOption));
        OnPropertyChanged(nameof(ShowFrostDragonBonesOption));
        OnPropertyChanged(nameof(CanDecreaseAccountCount));
        OnPropertyChanged(nameof(CanIncreaseAccountCount));
        if (synchronizingSelection || !initialized)
            return;

        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
        currentPrices = null;
        if (value is null)
        {
            selectionContext.Clear();
            ResetMethodDisplay();
            return;
        }

        SetAccountCount(accountCounts.GetValueOrDefault(
            value.Method.Definition.Slug,
            Math.Max(1, value.Method.Definition.Accounts)));
        ConfigureActionsPerHour(value);
        FlowRows.Clear();
        ErrorMessage = null;

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
        try
        {
            var definition = selected.Method.Definition;
            var prices = await marketData.GetLatestForAsync(definition.RequiredItemIds, cancellationToken);
            if (!ReferenceEquals(selected, SelectedMethod) || cancellationToken.IsCancellationRequested)
                return;

            currentPrices = prices;
            ApplyResult(GetEffectiveDefinition(selected), prices);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            if (ReferenceEquals(selected, SelectedMethod))
                ErrorMessage = "The method could not be priced because the Wiki market service is unavailable.";
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested && ReferenceEquals(selected, SelectedMethod))
                IsLoading = false;
        }
    }

    [RelayCommand]
    private void IncreaseAccountCount()
    {
        if (CanIncreaseAccountCount)
            AccountCount++;
    }

    [RelayCommand]
    private void DecreaseAccountCount()
    {
        if (CanDecreaseAccountCount)
            AccountCount--;
    }

    partial void OnAccountCountChanged(int value)
    {
        OnPropertyChanged(nameof(CanDecreaseAccountCount));
        OnPropertyChanged(nameof(CanIncreaseAccountCount));
        if (synchronizingAccountCount || SelectedMethod is null)
            return;

        accountCounts[SelectedMethod.Method.Definition.Slug] = value;
        if (currentPrices is not null)
            ApplyResult(GetEffectiveDefinition(SelectedMethod), currentPrices);
    }

    partial void OnUsingRegenPotionsChanged(bool value)
    {
        if (SelectedMethod?.Method is not VyrewatchMethod)
            return;

        var slug = SelectedMethod.Method.Definition.Slug;
        if (!actionRateOverrides.ContainsKey(slug))
            SetActionsPerHour(GetBaseDefinition(SelectedMethod).ActionsPerHour);
        UpdateActionsPerHourDisplay(SelectedMethod);

        if (currentPrices is not null)
            ApplyResult(GetEffectiveDefinition(SelectedMethod), currentPrices);
    }

    partial void OnPickingUpFrostDragonBonesChanged(bool value)
    {
        if (synchronizingMethodOptions || SelectedMethod?.Method is not FrostDragonMethod)
            return;

        if (currentPrices is not null)
            ApplyResult(GetEffectiveDefinition(SelectedMethod), currentPrices);
        _ = PersistBooleanOptionAsync(
            FrostDragonMethod.Slug,
            FrostDragonMethod.PickUpBonesOptionKey,
            value ? null : false);
    }

    partial void OnActionsPerHourChanged(decimal value)
    {
        if (synchronizingActionsPerHour || SelectedMethod is null)
            return;

        if (value <= 0m)
        {
            ErrorMessage = "Actions per hour must be greater than zero.";
            SetActionsPerHour(lastValidActionsPerHour);
            return;
        }

        if (ErrorMessage == "Actions per hour must be greater than zero.")
            ErrorMessage = null;
        lastValidActionsPerHour = value;
        var slug = SelectedMethod.Method.Definition.Slug;
        var defaultRate = GetBaseDefinition(SelectedMethod).ActionsPerHour;
        decimal? persistedOverride = value == defaultRate ? null : value;
        if (persistedOverride.HasValue)
            actionRateOverrides[slug] = persistedOverride.Value;
        else
            actionRateOverrides.Remove(slug);

        UpdateActionsPerHourDisplay(SelectedMethod);
        if (currentPrices is not null)
            ApplyResult(GetEffectiveDefinition(SelectedMethod), currentPrices);
        _ = PersistActionsPerHourOverrideAsync(slug, persistedOverride);
    }

    [RelayCommand]
    private void ResetActionsPerHour()
    {
        if (SelectedMethod is null)
            return;

        var slug = SelectedMethod.Method.Definition.Slug;
        actionRateOverrides.Remove(slug);
        SetActionsPerHour(GetBaseDefinition(SelectedMethod).ActionsPerHour);
        UpdateActionsPerHourDisplay(SelectedMethod);
        if (currentPrices is not null)
            ApplyResult(GetEffectiveDefinition(SelectedMethod), currentPrices);
        _ = PersistActionsPerHourOverrideAsync(slug, null);
    }

    private void ApplyResult(
        MoneyMakingMethodDefinition definition,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
        var result = calculator.Calculate(definition, prices, AccountCount);

        MethodKicker =
            $"{result.Method.ActionsPerHour:#,##0.##} actions / hour · {result.Method.Accounts} accounts";
        MethodName = result.Method.Name;
        MethodDescription = result.Method.Description;
        ProfitAllAccounts = DisplayFormat.Gp(result.ProfitAllAccounts);
        IsProfitPositive = result.ProfitAllAccounts >= 0;
        GrossSales = DisplayFormat.Gp(result.GrossRevenuePerAccount);
        Tax = $"− {DisplayFormat.Gp(result.TaxPerAccount)}";
        Supplies = $"− {DisplayFormat.Gp(result.InputCostPerAccount)}";
        ProfitPerAccount = DisplayFormat.Gp(result.ProfitPerAccount);
        AccountSummary = result.Method.Accounts == 1
            ? "across 1 account"
            : $"across {result.Method.Accounts} accounts";
        HasMissingPrices = result.HasMissingPrices;
        selectionContext.Select(
            result.Method.Slug,
            result.Method.Name,
            result.ProfitPerAccount,
            result.Method.Accounts,
            result.HasMissingPrices);

        FlowRows.Clear();
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

    private MoneyMakingMethodDefinition GetBaseDefinition(MoneyMethodRow selected) =>
        selected.Method switch
        {
            VyrewatchMethod => VyrewatchMethod.CreateDefinition(UsingRegenPotions),
            FrostDragonMethod => FrostDragonMethod.CreateDefinition(PickingUpFrostDragonBones),
            _ => selected.Method.Definition
        };

    private MoneyMakingMethodDefinition GetEffectiveDefinition(MoneyMethodRow selected)
    {
        var definition = GetBaseDefinition(selected);
        return definition with
        {
            ActionsPerHour = ActionsPerHour > 0m
                ? ActionsPerHour
                : definition.ActionsPerHour
        };
    }

    private void ConfigureActionsPerHour(MoneyMethodRow selected)
    {
        var definition = GetBaseDefinition(selected);
        var rate = actionRateOverrides.GetValueOrDefault(
            definition.Slug,
            definition.ActionsPerHour);
        SetActionsPerHour(rate);
        UpdateActionsPerHourDisplay(selected);
    }

    private void UpdateActionsPerHourDisplay(MoneyMethodRow selected)
    {
        var definition = GetBaseDefinition(selected);
        DefaultActionsPerHourText =
            $"Default: {definition.ActionsPerHour:#,##0.##} / hour";
        IsActionsPerHourOverridden =
            actionRateOverrides.ContainsKey(definition.Slug);
    }

    private async Task PersistActionsPerHourOverrideAsync(
        string slug,
        decimal? actionsPerHour)
    {
        try
        {
            await preferenceStore.SetActionsPerHourOverrideAsync(slug, actionsPerHour);
        }
        catch (Exception)
        {
            ErrorMessage =
                "The action-rate override is active for this session but could not be saved.";
        }
    }

    private async Task PersistBooleanOptionAsync(
        string slug,
        string optionKey,
        bool? value)
    {
        try
        {
            await preferenceStore.SetBooleanOptionAsync(slug, optionKey, value);
        }
        catch (Exception)
        {
            ErrorMessage =
                "The method option is active for this session but could not be saved.";
        }
    }

    private void OnSharedSelectionChanged(object? sender, EventArgs eventArgs)
    {
        if (synchronizingSelection || selectionContext.Current is not null)
            return;

        calculationCancellation?.Cancel();
        calculationCancellation?.Dispose();
        calculationCancellation = null;
        currentPrices = null;
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
        SetAccountCount(1);
        SetActionsPerHour(1m);
        DefaultActionsPerHourText = string.Empty;
        IsActionsPerHourOverridden = false;
        IsLoading = false;
        HasMissingPrices = false;
        ErrorMessage = null;
        FlowRows.Clear();
    }

    private void SetAccountCount(int value)
    {
        synchronizingAccountCount = true;
        AccountCount = Math.Max(1, value);
        synchronizingAccountCount = false;
    }

    private void SetActionsPerHour(decimal value)
    {
        synchronizingActionsPerHour = true;
        ActionsPerHour = value > 0m ? value : 1m;
        lastValidActionsPerHour = ActionsPerHour;
        synchronizingActionsPerHour = false;
    }
}
