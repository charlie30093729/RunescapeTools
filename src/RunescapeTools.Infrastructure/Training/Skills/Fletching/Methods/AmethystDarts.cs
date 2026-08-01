using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Fletching.Methods;

internal static class AmethystDarts
{
    private const decimal ExperiencePerDart = 21m;

    public static TrainingMethodDefinition Create()
    {
        var band = Band(
            5_346_332,
            1_000_000m,
            "Amethyst darts",
            new TrainingEconomics(
            [
                Input(Items.AmethystDartTip, 1m / ExperiencePerDart),
                Input(Items.Feather, 1m / ExperiencePerDart),
                Output(Items.AmethystDart, 1m / ExperiencePerDart)
            ]));
        return new TrainingMethodDefinition(
            "main-ehp",
            "Amethyst darts",
            FletchingGlobal.CreateRoute(band));
    }

    private static class Items
    {
        public static readonly CatalogueItem AmethystDartTip = new(25853, "Amethyst dart tip");
        public static readonly CatalogueItem Feather = new(314, "Feather");
        public static readonly CatalogueItem AmethystDart = new(25849, "Amethyst dart");
    }
}
