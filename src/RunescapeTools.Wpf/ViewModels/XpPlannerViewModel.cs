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
    private string gpPerHour = "Not priced";

    [ObservableProperty]
    private string pricingStatus = "Rate only";

    [ObservableProperty]
    private bool isProfit;

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

        var baseline = calculator.Calculate(definition, startExperience, targetExperience, prices);
        personalRate = preference?.ExperiencePerHourOverride ?? baseline.BaseRate;
        Recalculate();
    }

    public TrainingSkillDefinition Definition { get; }
    public string Skill => Definition.Skill;
    public string? IconUrl => OsrsSkillIconMap.GetIconUrl(Skill);
    public string? Note => Definition.Note;
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
            rateOverride);
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

    [RelayCommand]
    private void ResetRate()
    {
        var baseline = calculator.Calculate(Definition, StartExperience, TargetExperience, prices);
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
            var baseline = calculator.Calculate(Definition, value, TargetExperience, prices);
            suppressChanges = true;
            PersonalRate = baseline.BaseRate;
            suppressChanges = false;
        }

        ChangedAndRecalculate();
    }
    partial void OnTargetExperienceChanged(long value) => ChangedAndRecalculate();
    partial void OnPersonalRateChanged(decimal value) => ChangedAndRecalculate();

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
                PersonalRate > 0m ? PersonalRate : null);

            var activeBand = Definition.Bands
                .OrderBy(band => band.StartExperience)
                .LastOrDefault(band => band.StartExperience <= Result.StartExperience)
                ?? Definition.Bands.FirstOrDefault();
            Method = activeBand?.Method ?? "Passive / zero-time";
            Hours = Result.Hours.ToString("N1");
            TotalGp = Result.NetGp.HasValue ? DisplayFormat.Gp(Result.NetGp) : "Not priced";
            GpPerHour = Result.AverageGpPerHour.HasValue
                ? DisplayFormat.Gp(Result.AverageGpPerHour)
                : "Not priced";
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
    private readonly IMarketDataService marketData;
    private readonly ITrainingPlanStore store;
    private readonly ICurrentProfileContext profileContext;
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
    private string saveStatus = string.Empty;

    public XpPlannerViewModel(
        IEhpCatalogue catalogue,
        TrainingPlanCalculator calculator,
        IMarketDataService marketData,
        ITrainingPlanStore store,
        ICurrentProfileContext profileContext)
    {
        this.catalogue = catalogue;
        this.calculator = calculator;
        this.marketData = marketData;
        this.store = store;
        this.profileContext = profileContext;
        profileContext.ProfileChanged += (_, _) => initialized = false;
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
        var experience = Rows.Sum(row => row.Result.ExperienceRemaining);
        var hours = Rows.Sum(row => row.Result.Hours);
        var pricedExperience = Rows.Sum(row => row.Result.PricedExperience);
        var gp = Rows.Where(row => row.Result.NetGp.HasValue).Sum(row => row.Result.NetGp ?? 0m);
        TotalExperienceRemaining = DisplayFormat.Compact(experience);
        TotalHours = $"{hours:N1} h";
        TotalNetGp = pricedExperience > 0 ? DisplayFormat.Gp(gp) : "Not priced";
        PricedCoverage = experience > 0 ? $"{(decimal)pricedExperience / experience:P0}" : "100%";
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
