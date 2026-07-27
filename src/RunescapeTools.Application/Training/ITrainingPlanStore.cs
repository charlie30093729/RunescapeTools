namespace RunescapeTools.Application.Training;

public sealed record TrainingSkillPreference(
    string Skill,
    long TargetExperience,
    long? StartExperienceOverride = null,
    decimal? ExperiencePerHourOverride = null);

public interface ITrainingPlanStore
{
    Task<IReadOnlyDictionary<string, TrainingSkillPreference>> GetAsync(
        string rsn,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        string rsn,
        IReadOnlyCollection<TrainingSkillPreference> preferences,
        CancellationToken cancellationToken = default);
}
