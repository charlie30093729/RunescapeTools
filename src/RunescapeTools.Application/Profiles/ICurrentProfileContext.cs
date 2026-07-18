using RunescapeTools.Core.Profiles;

namespace RunescapeTools.Application.Profiles;

public interface ICurrentProfileContext
{
    string? CurrentRsn { get; }

    PlayerProfile? CurrentProfile { get; }

    bool HasLoadedProfile { get; }

    event EventHandler<CurrentProfileChangedEventArgs>? ProfileChanged;

    Task LoadSelectedProfileAsync(CancellationToken cancellationToken = default);

    Task LoadProfileAsync(string rsn, CancellationToken cancellationToken = default);

    Task RefreshAsync(CancellationToken cancellationToken = default);
}

public sealed class CurrentProfileChangedEventArgs(PlayerProfile profile) : EventArgs
{
    public PlayerProfile Profile { get; } = profile;
}
