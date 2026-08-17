using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Application.Market;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Application.Training;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.Training;
using RunescapeTools.Wpf.Services;

namespace RunescapeTools.Wpf.ViewModels;

public partial class TrainingMethodOptionViewModel(
    TrainingMethodDefinition definition) : ObservableObject
{
    public TrainingMethodDefinition Definition { get; } = definition;
    public string Id => Definition.Id;

    [ObservableProperty]
    private string name = definition.Name;

    public void UpdateName(
        long experience,
        TrainingMethodDefinition? configuredDefinition = null)
    {
        var source = configuredDefinition ?? Definition;
        if (source.UseStableDisplayName)
        {
            Name = source.Name;
            return;
        }

        var activeBand = source.Bands
            .OrderBy(band => band.StartExperience)
            .LastOrDefault(band => band.StartExperience <= experience)
            ?? source.Bands.FirstOrDefault();
        Name = activeBand?.Method ?? source.Name;
    }
}

public partial class XpPlannerRowViewModel : ObservableObject
{
    private readonly TrainingPlanCalculator calculator;
    private readonly Action changed;
    private readonly Action<XpPlannerRowViewModel>? configure;
    private readonly Action<XpPlannerRowViewModel>? showPricing;
    private IReadOnlyDictionary<int, ItemPrice> prices;
    private long pendingExperienceCredit;
    private bool hasPersonalRateOverride;
    private bool suppressChanges;

    [ObservableProperty]
    private long startExperience;

    [ObservableProperty]
    private long targetExperience;

    [ObservableProperty]
    private decimal personalRate;

    [ObservableProperty]
    private string method = string.Empty;

    [ObservableProperty]
    private string hours = "0";

    [ObservableProperty]
    private string totalGp = "Not priced";

    [ObservableProperty]
    private string economicRate = "Not priced";

    [ObservableProperty]
    private string creditSummary = string.Empty;

    [ObservableProperty]
    private bool hasExperienceCredit;

    [ObservableProperty]
    private string pricingStatus = "Rate only";

    [ObservableProperty]
    private bool isProfit;

    [ObservableProperty]
    private bool isMoneyMakingSelected;

    [ObservableProperty]
    private TrainingMethodOptionViewModel? selectedMethodOption;

    public XpPlannerRowViewModel(
        TrainingSkillDefinition definition,
        TrainingPlanCalculator calculator,
        long profileExperience,
        TrainingSkillPreference? preference,
        IReadOnlyDictionary<int, ItemPrice> prices,
        Action changed,
        Action<XpPlannerRowViewModel>? configure = null,
        Action<XpPlannerRowViewModel>? showPricing = null)
    {
        Definition = definition;
        this.calculator = calculator;
        this.prices = prices;
        this.changed = changed;
        this.configure = configure;
        this.showPricing = showPricing;
        ProfileExperience = Math.Max(0, profileExperience);
        startExperience = preference?.StartExperienceOverride ?? ProfileExperience;
        targetExperience = preference?.TargetExperience ?? TrainingPlanCalculator.MaximumExperience;
        isMoneyMakingSelected = preference?.IsMoneyMakingSelected ?? false;

        MethodOptions = definition.AvailableMethods
            .Select(method => new TrainingMethodOptionViewModel(method))
            .ToArray();
        selectedMethodOption = MethodOptions.FirstOrDefault(option =>
                                   string.Equals(
                                       option.Id,
                                       preference?.TrainingMethodId,
                                       StringComparison.OrdinalIgnoreCase))
                               ?? MethodOptions.FirstOrDefault(option =>
                                   string.Equals(
                                       option.Id,
                                       definition.DefaultMethodId,
                                       StringComparison.OrdinalIgnoreCase))
                               ?? MethodOptions.First();
        ConfigurationValues =
            definition.Configurator?.Definition.Normalize(preference?.Configuration).ToDictionary()
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var baseline = calculator.Calculate(
            definition,
            startExperience,
            targetExperience,
            prices,
            methodId: selectedMethodOption.Id,
            configuration: ConfigurationValues);
        hasPersonalRateOverride = preference?.ExperiencePerHourOverride is > 0m;
        personalRate = hasPersonalRateOverride
            ? preference!.ExperiencePerHourOverride!.Value
            : baseline.BaseRate;
        Recalculate();
    }

    public TrainingSkillDefinition Definition { get; }
    public string Skill => Definition.Skill;
    public string? IconUrl => OsrsSkillIconMap.GetIconUrl(Skill);
    public string? Note => Definition.Note;
    public IReadOnlyList<TrainingMethodDefinition> AvailableMethods => Definition.AvailableMethods;
    public IReadOnlyList<TrainingMethodOptionViewModel> MethodOptions { get; }
    public bool HasConfiguration => Definition.Configurator is not null;
    public Dictionary<string, string> ConfigurationValues { get; private set; }
    public long ProfileExperience { get; private set; }
    public TrainingSkillPlanResult Result { get; private set; } = null!;

    public TrainingSkillPreference ToPreference()
    {
        decimal? rateOverride = hasPersonalRateOverride ? PersonalRate : null;
        return new TrainingSkillPreference(
            Skill,
            TargetExperience,
            StartExperience == ProfileExperience ? null : StartExperience,
            rateOverride,
            IsMoneyMakingSelected,
            SelectedMethodOption?.Id,
            new Dictionary<string, string>(
                ConfigurationValues,
                StringComparer.OrdinalIgnoreCase));
    }

    public void UpdatePrices(IReadOnlyDictionary<int, ItemPrice> value)
    {
        prices = value;
        Recalculate();
    }

    public void ResetStart(long profileExperience)
    {
        ProfileExperience = Math.Max(0, profileExperience);
        StartExperience = ProfileExperience;
    }

    public void SetTarget(long value) => TargetExperience = value;

    public void SetPendingExperienceCredit(long value)
    {
        var normalized = Math.Max(0, value);
        if (pendingExperienceCredit == normalized)
            return;

        pendingExperienceCredit = normalized;
        Recalculate();
    }

    public void ApplyConfiguration(IReadOnlyDictionary<string, string> values)
    {
        var definition = Definition.Configurator?.Definition;
        if (definition is null)
            return;

        ConfigurationValues = definition.Normalize(values).ToDictionary();
        var baseline = calculator.Calculate(
            Definition,
            StartExperience,
            TargetExperience,
            prices,
            methodId: SelectedMethodOption?.Id,
            pendingExperienceCredit: pendingExperienceCredit,
            configuration: ConfigurationValues);
        if (!hasPersonalRateOverride)
        {
            suppressChanges = true;
            PersonalRate = baseline.BaseRate;
            suppressChanges = false;
        }

        ChangedAndRecalculate();
    }

    [RelayCommand]
    private void OpenConfiguration() => configure?.Invoke(this);

    [RelayCommand]
    private void OpenPriceDetails() => showPricing?.Invoke(this);

    [RelayCommand]
    private void ResetSkill()
    {
        suppressChanges = true;
        try
        {
            StartExperience = ProfileExperience;
            TargetExperience = TrainingPlanCalculator.MaximumExperience;
            IsMoneyMakingSelected = false;
            ConfigurationValues =
                Definition.Configurator?.Definition.Normalize().ToDictionary()
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            hasPersonalRateOverride = false;
            var baseline = calculator.Calculate(
                Definition,
                StartExperience,
                TargetExperience,
                prices,
                methodId: SelectedMethodOption?.Id,
                pendingExperienceCredit: pendingExperienceCredit,
                configuration: ConfigurationValues);
            PersonalRate = baseline.BaseRate;
        }
        finally
        {
            suppressChanges = false;
        }

        Recalculate();
        changed();
    }

    partial void OnStartExperienceChanged(long value)
    {
        if (suppressChanges)
            return;

        if (!hasPersonalRateOverride)
        {
            var baseline = calculator.Calculate(
                Definition,
                value,
                TargetExperience,
                prices,
                methodId: SelectedMethodOption?.Id,
                pendingExperienceCredit: pendingExperienceCredit,
                configuration: ConfigurationValues);
            suppressChanges = true;
            PersonalRate = baseline.BaseRate;
            suppressChanges = false;
        }

        ChangedAndRecalculate();
    }
    partial void OnTargetExperienceChanged(long value) => ChangedAndRecalculate();
    partial void OnPersonalRateChanged(decimal value)
    {
        if (!suppressChanges)
            hasPersonalRateOverride = value > 0m;
        ChangedAndRecalculate();
    }
    partial void OnIsMoneyMakingSelectedChanged(bool value) => changed();
    partial void OnSelectedMethodOptionChanged(TrainingMethodOptionViewModel? value)
    {
        if (suppressChanges || value is null)
            return;

        var baseline = calculator.Calculate(
            Definition,
            StartExperience,
            TargetExperience,
            prices,
            methodId: value.Id,
            pendingExperienceCredit: pendingExperienceCredit,
            configuration: ConfigurationValues);
        suppressChanges = true;
        hasPersonalRateOverride = false;
        PersonalRate = baseline.BaseRate;
        suppressChanges = false;
        ChangedAndRecalculate();
    }

    private void ChangedAndRecalculate()
    {
        if (suppressChanges)
            return;
        Recalculate();
        changed();
    }

    private void Recalculate()
    {
        suppressChanges = true;
        try
        {
            Result = calculator.Calculate(
                Definition,
                StartExperience,
                TargetExperience,
                prices,
                hasPersonalRateOverride && PersonalRate > 0m ? PersonalRate : null,
                SelectedMethodOption?.Id,
                pendingExperienceCredit: pendingExperienceCredit,
                configuration: ConfigurationValues);

            if (!hasPersonalRateOverride)
                PersonalRate = Result.BaseRate;

            foreach (var option in MethodOptions)
                option.UpdateName(
                    Result.EffectiveStartExperience,
                    string.Equals(
                        option.Id,
                        Result.Method.Id,
                        StringComparison.OrdinalIgnoreCase)
                        ? Result.Method
                        : null);

            var activeBand = Result.Method.Bands
                .OrderBy(band => band.StartExperience)
                .LastOrDefault(band => band.StartExperience <= Result.EffectiveStartExperience)
                ?? Result.Method.Bands.FirstOrDefault();
            Method = activeBand?.Method ?? "Passive / zero-time";
            Hours = Result.Hours == 0m ? "0" : Result.Hours.ToString("N1");
            TotalGp = Result.NetGp.HasValue ? DisplayFormat.Gp(Result.NetGp) : "Not priced";
            EconomicRate = !Result.IncludesActiveHours
                ? Result.GpPerExperience.HasValue
                    ? DisplayFormat.GpPerExperience(Result.GpPerExperience)
                    : "Not priced"
                : Result.AverageGpPerHour.HasValue
                    ? DisplayFormat.GpPerHour(Result.AverageGpPerHour)
                    : "Not priced";
            HasExperienceCredit = Result.AppliedExperienceCredit > 0;
            CreditSummary = HasExperienceCredit
                ? $"+{Result.AppliedExperienceCredit:N0} XP pending from Slayer"
                : string.Empty;
            IsProfit = Result.NetGp >= 0m;
            PricingStatus = Result.IsFullyPriced
                ? "Fully priced"
                : Result.PricedExperience > 0
                    ? $"{(decimal)Result.PricedExperience / Math.Max(1, Result.ExperienceRemaining):P2} priced"
                    : "Rate only";
        }
        finally
        {
            suppressChanges = false;
        }
    }

}

public partial class XpPlannerViewModel : ObservableObject, IPageViewModel
{
    private const long Level99Experience = 13_034_431;
    private readonly IEhpCatalogue catalogue;
    private readonly TrainingPlanCalculator calculator;
    private readonly TrainingMoneyMakingCalculator moneyMakingCalculator;
    private readonly IMarketDataService marketData;
    private readonly ITrainingPlanStore store;
    private readonly ICurrentProfileContext profileContext;
    private readonly MoneyMakerSelectionContext moneyMakerSelection;
    private readonly ITrainingConfigurationDialogService? configurationDialogs;
    private readonly ITrainingPriceDialogService? priceDialogs;
    private CancellationTokenSource? saveCancellation;
    private IReadOnlyDictionary<int, ItemPrice> prices = new Dictionary<int, ItemPrice>();
    private bool initialized;
    private bool suppressRowChanges;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string profileName = "No profile";

    [ObservableProperty]
    private string totalExperienceRemaining = "0";

    [ObservableProperty]
    private string totalHours = "0";

    [ObservableProperty]
    private string totalNetGp = "Not priced";

    [ObservableProperty]
    private string pricedCoverage = "0%";

    [ObservableProperty]
    private bool hasSelectedMoneyMaker;

    [ObservableProperty]
    private string selectedMoneyMakerName = "Choose a method";

    [ObservableProperty]
    private string selectedMoneyMakerRate = "Open Money Makers";

    [ObservableProperty]
    private string moneyMakerContributionSummary = "No money maker selected";

    [ObservableProperty]
    private bool isMoneyMakerProfitPositive = true;

    [ObservableProperty]
    private decimal selectedMoneyMakingHours;

    [ObservableProperty]
    private decimal moneyMakerGpContribution;

    [ObservableProperty]
    private string saveStatus = string.Empty;

    public XpPlannerViewModel(
        IEhpCatalogue catalogue,
        TrainingPlanCalculator calculator,
        TrainingMoneyMakingCalculator moneyMakingCalculator,
        IMarketDataService marketData,
        ITrainingPlanStore store,
        ICurrentProfileContext profileContext,
        MoneyMakerSelectionContext moneyMakerSelection,
        ITrainingConfigurationDialogService? configurationDialogs = null,
        ITrainingPriceDialogService? priceDialogs = null)
    {
        this.catalogue = catalogue;
        this.calculator = calculator;
        this.moneyMakingCalculator = moneyMakingCalculator;
        this.marketData = marketData;
        this.store = store;
        this.profileContext = profileContext;
        this.moneyMakerSelection = moneyMakerSelection;
        this.configurationDialogs = configurationDialogs;
        this.priceDialogs = priceDialogs;
        profileContext.ProfileChanged += (_, _) => initialized = false;
        moneyMakerSelection.SelectionChanged += OnMoneyMakerSelectionChanged;
        UpdateMoneyMakerDisplay();
    }

    public ObservableCollection<XpPlannerRowViewModel> Rows { get; } = [];
    public string CatalogueLabel => $"{catalogue.Version} · verified {catalogue.VerifiedOn:yyyy-MM-dd}";

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (initialized)
            return;

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            if (!profileContext.HasLoadedProfile)
                await profileContext.LoadSelectedProfileAsync(cancellationToken);

            var profile = profileContext.CurrentProfile
                          ?? throw new InvalidOperationException("Load a profile before opening the XP Planner.");
            var preferences = await store.GetAsync(profile.Rsn, cancellationToken);
            var itemIds = catalogue.Skills
                .SelectMany(skill => skill.AvailableMethods)
                .SelectMany(method => method.Bands)
                .Where(band => band.Economics is not null)
                .SelectMany(band => band.Economics!.Resources)
                .Select(resource => resource.ItemId)
                .Distinct();
            var priceLoadFailed = false;
            try
            {
                prices = await marketData.GetLatestForAsync(itemIds, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                prices = new Dictionary<int, ItemPrice>();
                priceLoadFailed = true;
            }

            suppressRowChanges = true;
            Rows.Clear();
            var profileSkills = profile.Skills.ToDictionary(skill => skill.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var definition in catalogue.Skills)
            {
                profileSkills.TryGetValue(definition.Skill, out var profileSkill);
                preferences.TryGetValue(definition.Skill, out var preference);
                Rows.Add(new XpPlannerRowViewModel(
                    definition,
                    calculator,
                    profileSkill?.Experience ?? 0,
                    preference,
                    prices,
                    OnRowChanged,
                    ConfigureRow,
                    ShowPriceDetails));
            }

            ProfileName = profile.Rsn;
            initialized = true;
            RecalculateSummary();
            if (priceLoadFailed)
                ErrorMessage = "Live GE prices are temporarily unavailable; the planner remains usable and affected methods are shown as unpriced.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            ErrorMessage = "The XP Planner could not load the selected profile or current GE prices.";
        }
        finally
        {
            suppressRowChanges = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshPricesAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var itemIds = catalogue.Skills
                .SelectMany(skill => skill.AvailableMethods)
                .SelectMany(method => method.Bands)
                .Where(band => band.Economics is not null)
                .SelectMany(band => band.Economics!.Resources)
                .Select(resource => resource.ItemId)
                .Distinct();
            prices = await marketData.GetLatestForAsync(itemIds, cancellationToken);
            foreach (var row in Rows)
                row.UpdatePrices(prices);
            RecalculateSummary();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            ErrorMessage = "Live GE prices are temporarily unavailable; the last valid calculation is still shown.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SetAllTo99() => SetAllTargets(Level99Experience);

    [RelayCommand]
    private void SetAllTo200M() => SetAllTargets(TrainingPlanCalculator.MaximumExperience);

    [RelayCommand]
    private void ResetFromProfile()
    {
        var skills = profileContext.CurrentProfile?.Skills
            .ToDictionary(skill => skill.Name, StringComparer.OrdinalIgnoreCase);
        if (skills is null)
            return;

        suppressRowChanges = true;
        foreach (var row in Rows)
        {
            skills.TryGetValue(row.Skill, out var skill);
            row.ResetStart(skill?.Experience ?? 0);
        }
        suppressRowChanges = false;
        OnRowChanged();
    }

    [RelayCommand]
    private Task SaveAsync(CancellationToken cancellationToken) => SaveNowAsync(cancellationToken);

    [RelayCommand]
    private void ResetMoneyMaker() => moneyMakerSelection.Clear();

    private void SetAllTargets(long target)
    {
        suppressRowChanges = true;
        foreach (var row in Rows)
            row.SetTarget(Math.Max(row.StartExperience, target));
        suppressRowChanges = false;
        OnRowChanged();
    }

    private void OnRowChanged()
    {
        if (suppressRowChanges || !initialized)
            return;
        RecalculateSummary();
        ScheduleSave();
    }

    private void ConfigureRow(XpPlannerRowViewModel row)
    {
        var definition = row.Definition.Configurator?.Definition;
        if (definition is null || configurationDialogs is null)
            return;

        var updated = configurationDialogs.Edit(
            row.Skill,
            row.Method,
            row.SelectedMethodOption?.Id,
            definition,
            row.ConfigurationValues);
        if (updated is not null)
            row.ApplyConfiguration(updated);
    }

    private void ShowPriceDetails(XpPlannerRowViewModel row) =>
        priceDialogs?.Show(row.Skill, row.Result, prices);

    private void RecalculateSummary()
    {
        ApplyExperienceDependencies();
        var experience = Rows.Sum(row => row.Result.ExperienceRemaining);
        var hours = Rows.Sum(row => row.Result.Hours);
        var pricedExperience = Rows.Sum(row => row.Result.PricedExperience);
        var gp = Rows.Where(row => row.Result.NetGp.HasValue).Sum(row => row.Result.NetGp ?? 0m);
        var moneyMaking = moneyMakingCalculator.Calculate(
            moneyMakerSelection.Current?.TotalProfitPerHour,
            Rows.Where(row => row.IsMoneyMakingSelected)
                .Select(row => row.Result.Hours));
        SelectedMoneyMakingHours = moneyMaking.SelectedHours;
        MoneyMakerGpContribution = moneyMaking.NetGp;
        gp += MoneyMakerGpContribution;
        TotalExperienceRemaining = DisplayFormat.Compact(experience);
        TotalHours = $"{hours:N1} h";
        TotalNetGp = pricedExperience > 0 || MoneyMakerGpContribution != 0m
            ? DisplayFormat.Gp(gp)
            : "Not priced";
        PricedCoverage = experience > 0 ? $"{(decimal)pricedExperience / experience:P0}" : "100%";
        MoneyMakerContributionSummary = moneyMakerSelection.Current is null
            ? "No money maker selected"
            : SelectedMoneyMakingHours <= 0m
                ? "Select skills below to apply this rate"
                : $"{DisplayFormat.Gp(MoneyMakerGpContribution)} over {SelectedMoneyMakingHours:N1} selected h";
    }

    private void ApplyExperienceDependencies()
    {
        var slayer = Rows.FirstOrDefault(row => row.Skill == "Slayer");
        var magic = Rows.FirstOrDefault(row => row.Skill == "Magic");
        if (magic is null)
            return;

        var generatedMagic = slayer?.Result.GeneratedExperience
            .GetValueOrDefault("Magic") ?? 0m;
        magic.SetPendingExperienceCredit((long)Math.Floor(generatedMagic));
    }

    private void OnMoneyMakerSelectionChanged(object? sender, EventArgs eventArgs)
    {
        UpdateMoneyMakerDisplay();
        RecalculateSummary();
    }

    private void UpdateMoneyMakerDisplay()
    {
        var selection = moneyMakerSelection.Current;
        HasSelectedMoneyMaker = selection is not null;
        SelectedMoneyMakerName = selection?.Name ?? "Choose a method";
        SelectedMoneyMakerRate = selection is null
            ? "Open Money Makers"
            : DisplayFormat.GpPerHour(selection.TotalProfitPerHour)
              + (selection.HasMissingPrices
                  ? $" | {selection.AccountCount} accounts | partial prices"
                  : selection.AccountCount == 1
                      ? " | 1 account"
                      : $" | {selection.AccountCount} accounts");
        IsMoneyMakerProfitPositive =
            selection is null || selection.TotalProfitPerHour >= 0m;
        if (selection is null)
        {
            SelectedMoneyMakingHours = 0m;
            MoneyMakerGpContribution = 0m;
            MoneyMakerContributionSummary = "No money maker selected";
        }
    }

    private void ScheduleSave()
    {
        saveCancellation?.Cancel();
        saveCancellation?.Dispose();
        saveCancellation = new CancellationTokenSource();
        var token = saveCancellation.Token;
        _ = SaveAfterDelayAsync(token);
    }

    private async Task SaveAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(600, cancellationToken);
            await SaveNowAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task SaveNowAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileContext.CurrentRsn))
            return;
        try
        {
            await store.SaveAsync(
                profileContext.CurrentRsn,
                Rows.Select(row => row.ToPreference()).ToArray(),
                cancellationToken);
            SaveStatus = $"Saved {DateTime.Now:t}";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            SaveStatus = "Could not save changes";
        }
    }
}
