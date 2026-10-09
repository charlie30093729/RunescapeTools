namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class MemoryMoneyMakingPreferenceStore : IMoneyMakingPreferenceStore
{
    public Dictionary<string, decimal> Overrides { get; } =
        new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Dictionary<string, bool>> BooleanOptions { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyDictionary<string, decimal>> GetActionsPerHourOverridesAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<string, decimal>>(
            new Dictionary<string, decimal>(Overrides, StringComparer.OrdinalIgnoreCase));

    public Task SetActionsPerHourOverrideAsync(
        string methodSlug,
        decimal? actionsPerHour,
        CancellationToken cancellationToken = default)
    {
        if (actionsPerHour.HasValue)
            Overrides[methodSlug] = actionsPerHour.Value;
        else
            Overrides.Remove(methodSlug);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<string, bool>> GetBooleanOptionsAsync(
        string methodSlug,
        CancellationToken cancellationToken = default)
    {
        if (BooleanOptions.TryGetValue(methodSlug, out var options))
        {
            return Task.FromResult<IReadOnlyDictionary<string, bool>>(
                new Dictionary<string, bool>(options, StringComparer.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyDictionary<string, bool>>(
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase));
    }

    public Task SetBooleanOptionAsync(
        string methodSlug,
        string optionKey,
        bool? value,
        CancellationToken cancellationToken = default)
    {
        if (!BooleanOptions.TryGetValue(methodSlug, out var options))
        {
            options = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            BooleanOptions[methodSlug] = options;
        }

        if (value.HasValue)
            options[optionKey] = value.Value;
        else
            options.Remove(optionKey);

        if (options.Count == 0)
            BooleanOptions.Remove(methodSlug);
        return Task.CompletedTask;
    }
}
