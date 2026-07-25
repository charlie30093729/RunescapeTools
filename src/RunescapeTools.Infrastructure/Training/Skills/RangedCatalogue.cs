using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class RangedCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Ranged",
            Band(0, 250_000m, "Bonus XP from Slayer"),
            Band(6_517_253, 330_000m, "Bonus XP from Slayer"),
            Band(13_034_431, 1_325_000m, "Chinning maniacal monkeys"));
}
