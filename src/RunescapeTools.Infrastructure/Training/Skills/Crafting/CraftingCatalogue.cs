using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class CraftingCatalogue
{
    private const decimal BlackDhideBodyCraftingXp = 258m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Crafting",
            Band(0, 37_000m, "Leather items"),
            Band(4_470, 139_000m, "Sapphires"),
            Band(9_730, 187_650m, "Emeralds"),
            Band(20_224, 236_300m, "Rubies"),
            Band(50_339, 298_850m, "Diamonds"),
            Band(368_599, 335_230m, "Green d'hide bodies"),
            Band(814_445, 378_490m, "Blue d'hide bodies"),
            Band(1_475_581, 421_740m, "Red d'hide bodies"),
            Band(
                2_951_373,
                465_000m,
                "Black dragonhide bodies",
                new TrainingEconomics(
                    [
                        Input(Items.BlackDragonLeather, 3m / BlackDhideBodyCraftingXp),
                        Output(Items.BlackDhideBody, 1m / BlackDhideBodyCraftingXp)
                    ])));

    private static class Items
    {
        public static readonly CatalogueItem BlackDragonLeather = new(2509, "Black dragon leather");
        public static readonly CatalogueItem BlackDhideBody = new(2503, "Black d'hide body");
    }
}
