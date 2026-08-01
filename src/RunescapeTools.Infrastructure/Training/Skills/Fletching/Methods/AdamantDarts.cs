using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Fletching.Methods;

internal static class AdamantDarts
{
    private const decimal ExperiencePerDart = 15m;

    public static TrainingMethodDefinition Create()
    {
        var band = Band(
            737_627,
            300_000m,
            "Adamant darts",
            new TrainingEconomics(
            [
                Input(Items.AdamantDartTip, 1m / ExperiencePerDart),
                Input(Items.Feather, 1m / ExperiencePerDart),
                Output(Items.AdamantDart, 1m / ExperiencePerDart)
            ]));
        return new TrainingMethodDefinition(
            "adamant-darts",
            "Adamant darts",
            FletchingGlobal.CreateRoute(band),
            "Requires level 67 Fletching and completion of The Tourist Trap. The standard rate assumes 20,000 darts completed per hour.");
    }

    private static class Items
    {
        public static readonly CatalogueItem AdamantDartTip = new(823, "Adamant dart tip");
        public static readonly CatalogueItem Feather = new(314, "Feather");
        public static readonly CatalogueItem AdamantDart = new(810, "Adamant dart");
    }
}
