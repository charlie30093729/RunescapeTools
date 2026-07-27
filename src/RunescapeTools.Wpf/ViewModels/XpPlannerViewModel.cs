using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Application.Market;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Application.Training;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.Training;

namespace RunescapeTools.Wpf.ViewModels;

public partial class XpPlannerRowViewModel : ObservableObject
{
    private readonly TrainingPlanCalculator calculator;
    private readonly Action changed;
    private IReadOnlyDictionary<int, ItemPrice> prices;
    private long pendingExperienceCredit;
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
    private string economicRateToolTip = "Estimated GP per hour.";

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

    public XpPlannerRowViewModel(
        TrainingSkillDefinition definition,
        TrainingPlanCalculator calculator,
        long profileExperience,
        TrainingSkillPreference? preference,
        IReadOnlyDictionary<int, ItemPrice> prices,
        Action changed)
    {
        Definition = definition;
        this.calculator = calculator;
        this.prices = prices;
        this.changed = changed;
        ProfileExperience = Math.Max(0, profileExperience);
        startExperience = preference?.StartExperienceOverride ?? ProfileExperience;
        targetExperience = preference?.TargetExperience ?? TrainingPlanCalculator.MaximumExperience;
        isMoneyMakingSelected = preference?.IsMoneyMakingSelected ?? false;

        var baseline = calculator.Calculate(definition, startExperience, targetExperience, prices);
        personalRate = preference?.ExperiencePerHourOverride ?? baseline.BaseRate;
        Recalculate();
    }

    public TrainingSkillDefinition Definition { get; }
    public string Skill => Definition.Skill;
    public string? IconUrl => OsrsSkillIconMap.GetIconUrl(Skill);
    public string? Note => Definition.Note;
    public IReadOnlyList<TrainingMethodDefinition> AvailableMethods => Definition.AvailableMethods;
    public long ProfileExperience { get; private set; }
    public TrainingSkillPlanResult Result { get; private set; } = null!;

    public TrainingSkillPreference ToPreference()
    {
        decimal? rateOverride = Result.BaseRate > 0m && Math.Abs(PersonalRate - Result.BaseRate) < 0.001m
            ? null
            : PersonalRate;
        return new TrainingSkillPreference(
            Skill,
            TargetExperience,
            StartExperience == ProfileExperience ? null : StartExperience,
            rateOverride,
            IsMoneyMakingSelected);
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

    [RelayCommand]
    private void ResetRate()
    {
        var baseline = calculator.Calculate(
            Definition,
            StartExperience,
            TargetExperience,
            prices,
            pendingExperienceCredit: pendingExperienceCredit);
        PersonalRate = baseline.BaseRate;
    }

    partial void OnStartExperienceChanged(long value)
    {
        if (suppressChanges)
            return;

        var wasUsingCatalogueRate = Result is not null
                                    && Math.Abs(PersonalRate - Result.BaseRate) < 0.001m;
        if (wasUsingCatalogueRate)
        {
            var baseline = calculator.Calculate(
                Definition,
                value,
                TargetExperience,
                prices,
                pendingExperienceCredit: pendingExperienceCredit);
            suppressChanges = true;
            PersonalRate = baseline.BaseRate;
            suppressChanges = false;
        }

        ChangedAndRecalculate();
    }
    partial void OnTargetExperienceChanged(long value) => ChangedAndRecalculate();
    partial void OnPersonalRateChanged(decimal value) => ChangedAndRecalculate();
    partial void OnIsMoneyMakingSelectedChanged(bool value) => changed();

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
                PersonalRate > 0m ? PersonalRate : null,
                pendingExperienceCredit: pendingExperienceCredit);

            var activeBand = Result.Method.Bands
                .OrderBy(band => band.StartExperience)
                .LastOrDefault(band => band.StartExperience <= Result.EffectiveStartExperience)
                ?? Result.Method.Bands.FirstOrDefault();
            Method = activeBand?.Method ?? "Passive / zero-time";
            Hours = Result.Hours == 0m ? "0" : Result.Hours.ToString("N1");
            TotalGp = Result.NetGp.HasValue ? DisplayFormat.Gp(Result.NetGp) : "Not priced";
            EconomicRate = Definition.IsZeroTime
                ? Result.GpPerExperience.HasValue
                    ? DisplayFormat.GpPerExperience(Result.GpPerExperience)
                    : "Not priced"
                : Result.AverageGpPerHour.HasValue
                    ? DisplayFormat.GpPerHour(Result.AverageGpPerHour)
                    : "Not priced";
            EconomicRateToolTip = Definition.IsZeroTime
                ? "Estimated GP per XP. This method contributes zero active hours."
                : "Estimated GP per hour. Negative values are costs; positive values are profit.";
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
        MoneyMakerSelectionContext moneyMakerSelection)
    {
        this.catalogue = catalogue;
        this.calculator = calculator;
        this.moneyMakingCalculator = moneyMakingCalculator;
        this.marketData = marketData;
        this.store = store;
        this.profileContext = profileContext;
        this.moneyMakerSelection = moneyMakerSelection;
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
                .SelectMany(skill => skill.Bands)
                .Where(band => band.Economics is not null)
                .SelectMany(band => band.Economics!.Resources)
                .Select(resource => resource.ItemId)
                .Distinct();
            prices = await marketData.GetLatestForAsync(itemIds, cancellationToken);

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
                    OnRowChanged));
            }

            ProfileName = profile.Rsn;
            initialized = true;
            RecalculateSummary();
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
            var itemIds = catalogue.Skills.SelectMany(skill => skill.Bands)
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

    private void RecalculateSummary()
    {
        ApplyExperienceDependencies();
        var experience = Rows.Sum(row => row.Result.ExperienceRemaining);
        var hours = Rows.Sum(row => row.Result.Hours);
        var pricedExperience = Rows.Sum(row => row.Result.PricedExperience);
        var gp = Rows.Where(row => row.Result.NetGp.HasValue).Sum(row => row.Result.NetGp ?? 0m);
        var moneyMaking = moneyMakingCalculator.Calculate(
            moneyMakerSelection.Current?.ProfitPerAccountPerHour,
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
            : DisplayFormat.GpPerHour(selection.ProfitPerAccountPerHour)
              + (selection.HasMissingPrices ? " | partial prices" : " per account");
        IsMoneyMakerProfitPositive =
            selection is null || selection.ProfitPerAccountPerHour >= 0m;
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
