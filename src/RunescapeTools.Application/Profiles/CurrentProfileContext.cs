using RunescapeTools.Core.Profiles;

namespace RunescapeTools.Application.Profiles;

public sealed class CurrentProfileContext(
    IHiscoreClient hiscoreClient,
    HiscoreParser parser,
    IProfilePreferenceStore preferenceStore) : ICurrentProfileContext
{
    private readonly SemaphoreSlim loadGate = new(1, 1);

    public string? CurrentRsn => CurrentProfile?.Rsn;

    public PlayerProfile? CurrentProfile { get; private set; }

    public bool HasLoadedProfile => CurrentProfile is not null;

    public event EventHandler<CurrentProfileChangedEventArgs>? ProfileChanged;

    public async Task LoadSelectedProfileAsync(CancellationToken cancellationToken = default)
    {
        var rsn = await preferenceStore.GetSelectedRsnAsync(cancellationToken);
        await LoadProfileAsync(rsn, cancellationToken);
    }

    public async Task LoadProfileAsync(string rsn, CancellationToken cancellationToken = default)
    {
        var trimmedRsn = rsn?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRsn))
            throw new ArgumentException("Enter an Old School RuneScape account name.", nameof(rsn));

        await loadGate.WaitAsync(cancellationToken);
        try
        {
            var rawResponse = await hiscoreClient.GetRawHiscoresAsync(trimmedRsn, cancellationToken);
            var loadedProfile = parser.Parse(trimmedRsn, rawResponse);

            // Persist before publishing the new state so a failed write cannot replace a valid profile.
            await preferenceStore.SetSelectedRsnAsync(loadedProfile.Rsn, cancellationToken);
            CurrentProfile = loadedProfile;
            ProfileChanged?.Invoke(this, new CurrentProfileChangedEventArgs(loadedProfile));
        }
        finally
        {
            loadGate.Release();
        }
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var currentRsn = CurrentRsn;
        if (string.IsNullOrWhiteSpace(currentRsn))
            throw new InvalidOperationException("No profile has been loaded yet.");

        return LoadProfileAsync(currentRsn, cancellationToken);
    }
}
