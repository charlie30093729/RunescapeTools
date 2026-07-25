using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class SailingCatalogue
{
    private const decimal ReviewedShardTotal = 16_040m;
    private const decimal ReviewedPotionTotal = 40_100m;
    private const decimal ReviewedExperiencePerHour = 240_000m;
    private const decimal ReviewedHours =
        TrainingPlanCalculator.MaximumExperience / ReviewedExperiencePerHour;
    private const decimal PotionsPerHour = 48.12m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Sailing",
            Band(
                0,
                ReviewedExperiencePerHour,
                "Gwenith Glide - rosewood hull",
                new TrainingEconomics(
                    [
                        Input(
                            SuperCombatPotion4,
                            "Super combat potion(4)",
                            0m,
                            PotionsPerHour),
                        Output(
                            DivineSuperCombatPotion4,
                            "Divine super combat potion(4)",
                            0m,
                            PotionsPerHour)
                    ])),
            $"Reviewed 0-200m projection: {ReviewedShardTotal:N0} crystal shards gained and " +
            $"{ReviewedPotionTotal:N0} divine super combat potions produced over {ReviewedHours:N2} active hours. " +
            "The crystal extractor converts each shard into 10 dust; four dust upgrades one potion. " +
            "No multiskilling is included.");
}
