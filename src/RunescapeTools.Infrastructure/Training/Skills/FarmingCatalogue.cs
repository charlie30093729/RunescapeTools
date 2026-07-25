using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class FarmingCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Farming",
            "Efficient tree-run rates represent active player time. Seed, protection-payment, and harvest economics are not yet priced.",
            Band(0, 16_000m, "Quests"),
            Band(32_500, 364_000m, "Efficient tree runs"),
            Band(61_512, 575_000m, "Efficient tree runs"),
            Band(166_636, 841_000m, "Efficient tree runs"),
            Band(273_742, 1_222_000m, "Efficient tree runs"),
            Band(605_032, 1_428_000m, "Efficient tree runs"),
            Band(1_210_421, 2_063_000m, "Efficient tree runs"),
            Band(2_192_818, 2_475_000m, "Efficient tree runs"),
            Band(3_258_594, 2_611_000m, "Efficient tree runs"),
            Band(6_517_253, 2_669_000m, "Efficient tree runs"));
}
