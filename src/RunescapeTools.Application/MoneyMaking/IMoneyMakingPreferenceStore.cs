namespace RunescapeTools.Application.MoneyMaking;

public interface IMoneyMakingPreferenceStore
{
    Task<IReadOnlyDictionary<string, decimal>> GetActionsPerHourOverridesAsync(
        CancellationToken cancellationToken = default);

    Task SetActionsPerHourOverrideAsync(
        string methodSlug,
        decimal? actionsPerHour,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, bool>> GetBooleanOptionsAsync(
        string methodSlug,
        CancellationToken cancellationToken = default);

    Task SetBooleanOptionAsync(
        string methodSlug,
        string optionKey,
        bool? value,
        CancellationToken cancellationToken = default);
}
