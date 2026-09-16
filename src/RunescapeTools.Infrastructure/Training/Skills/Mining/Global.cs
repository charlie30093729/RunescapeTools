using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Mining;

internal static class MiningGlobal
{
    public const string ProspectorOutfitKey = "prospector-outfit";
    public const decimal ProspectorExperienceMultiplier = 1.025m;
    public static ITrainingSkillConfigurator Configurator { get; } = new TrainingSkillConfigurator(
        new TrainingConfigurationDefinition(
        [
            new TrainingConfigurationOption(ProspectorOutfitKey, "Full Prospector outfit",
                TrainingConfigurationOptionKind.Toggle, bool.TrueString,
                "Apply 2.5% more Mining XP to Calcified rocks, including personal base rates. " +
                "Fewer charges and rewards are needed for the same XP goal. Existing granite rates are unchanged.",
                ApplicableMethodIds: ["calcified-rocks-crystal-pickaxe"])
        ]),
        (method, values) => method.Id == "calcified-rocks-crystal-pickaxe"
            && values.GetToggle(ProspectorOutfitKey)
                ? TrainingConfigurationTransforms.ApplyExperienceMultiplier(method,
                    ProspectorExperienceMultiplier,
                    band => band.StartExperience >= CrystalPickaxeUnlockExperience)
                : method);

    public const long CrystalPickaxeUnlockExperience = 814_445; // Level 71, unboosted.
    public const decimal CrystalChargesPerShard = 100m;
    public const decimal CrystalShardsPerEnhancedSeed = 150m;
    public const decimal CrystalChargesPerEnhancedSeed =
        CrystalChargesPerShard * CrystalShardsPerEnhancedSeed;

    public static IReadOnlyList<TrainingRateBand> WithMainRouteBeforeUnlock(
        IReadOnlyList<TrainingRateBand> bands) =>
        Methods.MainEhp.Create().Bands
            .Where(band => band.StartExperience < bands[0].StartExperience)
            .Concat(bands)
            .ToArray();
}
