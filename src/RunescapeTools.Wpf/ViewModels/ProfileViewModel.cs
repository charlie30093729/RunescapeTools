using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Core.Profiles;

namespace RunescapeTools.Wpf.ViewModels;

public partial class ProfileViewModel : ObservableObject, IPageViewModel
{
    private readonly ICurrentProfileContext profileContext;
    private bool initialLoadAttempted;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string searchRsn = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfile))]
    [NotifyPropertyChangedFor(nameof(ProfileRsn))]
    [NotifyPropertyChangedFor(nameof(OverallRank))]
    [NotifyPropertyChangedFor(nameof(TotalLevel))]
    [NotifyPropertyChangedFor(nameof(TotalExperience))]
    [NotifyPropertyChangedFor(nameof(RetrievedAt))]
    private PlayerProfile? profile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private IReadOnlyList<ProfileSkillDisplay> skills = [];

    public ProfileViewModel(ICurrentProfileContext profileContext)
    {
        this.profileContext = profileContext;
        profileContext.ProfileChanged += OnProfileChanged;
        if (profileContext.CurrentProfile is not null)
            ApplyProfile(profileContext.CurrentProfile);
    }

    public bool HasProfile => Profile is not null;
    public string ProfileRsn => Profile?.Rsn ?? "No profile loaded";
    public string OverallRank => FormatRank(Profile?.OverallRank);
    public string TotalLevel => Profile?.TotalLevel.ToString("N0") ?? "—";
    public string TotalExperience => Profile is null ? "—" : $"{Profile.TotalExperience:N0} xp";
    public string RetrievedAt => Profile is null
        ? "Not refreshed yet"
        : $"Updated {Profile.RetrievedAtUtc.ToLocalTime():ddd, d MMM yyyy h:mm tt}";

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (initialLoadAttempted)
        {
            if (profileContext.CurrentProfile is not null)
                ApplyProfile(profileContext.CurrentProfile);
            return;
        }

        initialLoadAttempted = true;
        await ExecuteLoadAsync(
            () => profileContext.LoadSelectedProfileAsync(cancellationToken),
            cancellationToken);
    }

    private bool CanSearch() => !IsLoading && !string.IsNullOrWhiteSpace(SearchRsn);

    private bool CanRefresh() => !IsLoading && profileContext.HasLoadedProfile;

    [RelayCommand(CanExecute = nameof(CanSearch), IncludeCancelCommand = true)]
    private Task SearchAsync(CancellationToken cancellationToken)
    {
        var rsn = SearchRsn.Trim();
        return ExecuteLoadAsync(
            () => profileContext.LoadProfileAsync(rsn, cancellationToken),
            cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanRefresh), IncludeCancelCommand = true)]
    private Task RefreshAsync(CancellationToken cancellationToken) =>
        ExecuteLoadAsync(
            () => profileContext.RefreshAsync(cancellationToken),
            cancellationToken);

    private async Task ExecuteLoadAsync(Func<Task> load, CancellationToken cancellationToken)
    {
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await load();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (PlayerNotFoundException exception)
        {
            ErrorMessage = $"No normal-account hiscores were found for ‘{exception.Rsn}’. Check the RSN and try again.";
        }
        catch (HiscoreParseException)
        {
            ErrorMessage = "The Hiscores service returned incomplete or malformed profile data. Your current profile was kept.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "The Hiscores request timed out. Your current profile was kept.";
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "The Hiscores service could not be reached. Check your connection and try again.";
        }
        catch (Exception exception)
        {
            ErrorMessage = $"The profile could not be loaded. {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnProfileChanged(object? sender, CurrentProfileChangedEventArgs args)
    {
        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher
            && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => ApplyProfile(args.Profile));
            return;
        }

        ApplyProfile(args.Profile);
    }

    private void ApplyProfile(PlayerProfile loadedProfile)
    {
        Profile = loadedProfile;
        SearchRsn = loadedProfile.Rsn;
        Skills = loadedProfile.Skills
            .OrderBy(skill => skill.ApiOrder)
            .Select(skill => new ProfileSkillDisplay(
                skill.Name,
                skill.Level.ToString("N0"),
                skill.Experience < 0 ? "Not ranked" : $"{skill.Experience:N0} xp",
                FormatRank(skill.Rank),
                DisplayFormat.Monogram(skill.Name)))
            .ToArray();
        RefreshCommand.NotifyCanExecuteChanged();
    }

    private static string FormatRank(int? rank) => rank switch
    {
        null => "—",
        < 0 => "Unranked",
        _ => $"Rank {rank.Value:N0}"
    };
}

public sealed record ProfileSkillDisplay(
    string Name,
    string Level,
    string Experience,
    string Rank,
    string Monogram);
