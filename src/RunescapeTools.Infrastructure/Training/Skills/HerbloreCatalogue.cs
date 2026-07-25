using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class HerbloreCatalogue
{
    private const decimal SaradominBrewHerbloreXp = 180m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Herblore",
            Band(0, 11_100m, "Quests"),
            Band(8_025, 218_750m, "Serum 207s"),
            Band(123_660, 293_750m, "Super energies"),
            Band(166_636, 312_500m, "Super strengths"),
            Band(368_599, 356_250m, "Super restores"),
            Band(496_254, 375_000m, "Super defences"),
            Band(668_051, 393_750m, "Antifire potions"),
            Band(899_257, 406_250m, "Ranging potions"),
            Band(1_336_443, 431_250m, "Magic potions"),
            Band(1_475_581, 535_500m, "1t stamina potions"),
            Band(
                2_192_818,
                450_000m,
                "Saradomin brews",
                new TrainingEconomics(
                    [
                        Input(
                            ToadflaxPotionUnfinished,
                            "Toadflax potion (unf)",
                            1m / SaradominBrewHerbloreXp),
                        Input(CrushedNest, "Crushed nest", 1m / SaradominBrewHerbloreXp),
                        Output(SaradominBrew3, "Saradomin brew(3)", 1m / SaradominBrewHerbloreXp)
                    ])));
}
