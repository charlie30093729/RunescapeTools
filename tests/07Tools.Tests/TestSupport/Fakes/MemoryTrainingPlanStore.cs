namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class MemoryTrainingPlanStore : ITrainingPlanStore
{
    private readonly Dictionary<string, Dictionary<string, TrainingSkillPreference>> profiles =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyDictionary<string, TrainingSkillPreference>> GetAsync(
        string rsn,
        CancellationToken cancellationToken = default)
    {
        if (profiles.TryGetValue(rsn.Trim(), out var values))
            return Task.FromResult<IReadOnlyDictionary<string, TrainingSkillPreference>>(values);
        return Task.FromResult<IReadOnlyDictionary<string, TrainingSkillPreference>>(
            new Dictionary<string, TrainingSkillPreference>(StringComparer.OrdinalIgnoreCase));
    }

    public Task SaveAsync(
        string rsn,
        IReadOnlyCollection<TrainingSkillPreference> preferences,
        CancellationToken cancellationToken = default)
    {
        profiles[rsn.Trim()] = preferences.ToDictionary(value => value.Skill, StringComparer.OrdinalIgnoreCase);
        return Task.CompletedTask;
    }
}
