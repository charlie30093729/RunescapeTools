using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Crafting.Methods;

internal static class BlackDragonhideBodies
{
    private const decimal ExperiencePerBody = 258m;

    public static TrainingMethodDefinition Create()
    {
        var band = Band(
            2_951_373,
            465_000m,
            "Black dragonhide bodies",
            new TrainingEconomics(
            [
                Input(Items.BlackDragonLeather, 3m / ExperiencePerBody),
                Output(Items.BlackDhideBody, 1m / ExperiencePerBody)
            ]));
        return new TrainingMethodDefinition(
            "main-ehp",
            "Black dragonhide bodies",
            CraftingGlobal.CreateRoute(band));
    }

    private static class Items
    {
        public static readonly CatalogueItem BlackDragonLeather = new(2509, "Black dragon leather");
        public static readonly CatalogueItem BlackDhideBody = new(2503, "Black d'hide body");
    }
}
