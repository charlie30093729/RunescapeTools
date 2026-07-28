namespace RunescapeTools.Application.MoneyMaking;

public interface IMoneyMakingPreferenceStore
{
    Task<IReadOnlyDictionary<string, decimal>> GetActionsPerHourOverridesAsync(
        CancellationToken cancellationToken = default);

    Task SetActionsPerHourOverrideAsync(
        string methodSlug,
        decimal? actionsPerHour,
        CancellationToken cancellationToken = default);
}
