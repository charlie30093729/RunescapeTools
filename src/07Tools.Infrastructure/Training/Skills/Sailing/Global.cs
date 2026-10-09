using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Sailing;

internal static class SailingGlobal
{
    public const string CrystalExtractorKey = "crystal-extractor";
    public const long CrystalExtractorUnlockExperience = 992_895; // Level 73.
    public const decimal CrystalExtractorExperiencePerHour = 250m * 3_600m / 63m;

    public static ITrainingSkillConfigurator Configurator { get; } = new TrainingSkillConfigurator(
        new TrainingConfigurationDefinition(
        [
            new TrainingConfigurationOption(CrystalExtractorKey, "Use crystal extractor while salvaging",
                TrainingConfigurationOptionKind.Toggle, bool.TrueString,
                "Adds about 14,286 Sailing XP/hour from level 73 (250 XP every 63 seconds). " +
                "Requires 67 Construction and the installed facility. Applies to Salvaging only; " +
                "personal base XP rates receive the same fixed bonus.",
                ApplicableMethodIds: ["salvaging"])
        ]),
        (method, values) => method.Id == "salvaging"
            ? Methods.Salvaging.Create(values.GetToggle(CrystalExtractorKey))
            : method);
}
