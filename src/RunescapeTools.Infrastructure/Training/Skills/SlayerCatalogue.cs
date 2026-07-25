using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class SlayerCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Slayer",
            Band(0, 5_000m, "Efficient Slayer"),
            Band(37_224, 12_000m, "Efficient Slayer"),
            Band(101_333, 40_000m, "Efficient Slayer"),
            Band(449_428, 74_250m, "Efficient Slayer"),
            Band(1_986_068, 79_000m, "Efficient Slayer"),
            Band(3_258_594, 86_500m, "Efficient Slayer"),
            Band(5_346_332, 87_000m, "Efficient Slayer"),
            Band(7_195_629, 93_000m, "Efficient Slayer"),
            Band(13_034_431, 110_900m, "Efficient Slayer"));
}
