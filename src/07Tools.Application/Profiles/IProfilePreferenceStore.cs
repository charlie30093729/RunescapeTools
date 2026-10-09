namespace RunescapeTools.Application.Profiles;

public interface IProfilePreferenceStore
{
    Task<string> GetSelectedRsnAsync(CancellationToken cancellationToken = default);

    Task SetSelectedRsnAsync(string rsn, CancellationToken cancellationToken = default);
}
