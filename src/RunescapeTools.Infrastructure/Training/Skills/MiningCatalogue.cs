using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class MiningCatalogue
{
    private const decimal InfernalPickaxeExperiencePerDragonPickaxe = 960_000m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Mining",
            "Granite is dropped. Infernal pickaxe recharges use one dragon pickaxe per 960,000 Mining XP.",
            Band(0, 20_000m, "Quests"),
            Band(35_025, 50_000m, "Prospector and celestial ring"),
            GraniteBand(393_485, 106_540m),
            GraniteBand(1_210_421, 112_166m),
            GraniteBand(3_258_594, 116_760m),
            GraniteBand(8_771_558, 119_438m),
            GraniteBand(13_034_431, 126_000m));

    private static TrainingRateBand GraniteBand(long startExperience, decimal experiencePerHour) =>
        Band(
            startExperience,
            experiencePerHour,
            "3t4g granite - infernal pickaxe",
            new TrainingEconomics(
                [
                    Input(
                        DragonPickaxe,
                        "Dragon pickaxe (infernal pickaxe recharge)",
                        1m / InfernalPickaxeExperiencePerDragonPickaxe)
                ]));
}
