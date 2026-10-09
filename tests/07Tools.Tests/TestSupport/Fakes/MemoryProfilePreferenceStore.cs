namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class MemoryProfilePreferenceStore(string selectedRsn) : IProfilePreferenceStore
{
    public string SelectedRsn { get; private set; } = selectedRsn;

    public Task<string> GetSelectedRsnAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(SelectedRsn);

    public Task SetSelectedRsnAsync(string rsn, CancellationToken cancellationToken = default)
    {
        SelectedRsn = rsn.Trim();
        return Task.CompletedTask;
    }
}
