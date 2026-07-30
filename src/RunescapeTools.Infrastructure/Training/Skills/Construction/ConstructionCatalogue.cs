using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction;

internal static class ConstructionCatalogue
{
    private const decimal DemonButlerGpPerTrip = 10_000m / 8m;
    private const decimal DemonButlerCapacity = 24m;

    public static TrainingSkillDefinition Create()
    {
        var oakEconomics = PlankEconomics(Items.OakPlank, 60m);
        var mahoganyEconomics = PlankEconomics(Items.MahoganyPlank, 140m);
        return new TrainingSkillDefinition(
            "Construction",
            [
                Band(0, 54_700m, "Low-level furniture"),
                Band(18_247, 200_000m, "Oak larders", oakEconomics),
                Band(37_224, 290_000m, "Mahogany bookcases", mahoganyEconomics),
                Band(123_660, 950_000m, "Mahogany tables", mahoganyEconomics),
                Band(1_475_581, 1_070_000m, "Mahogany benches", mahoganyEconomics),
                Band(13_034_431, 1_440_000m, "2t mahogany flatpacks", mahoganyEconomics)
            ],
            Note: "Carpenter's outfit follows the saved Construction configuration.",
            Configurator: ConstructionGlobal.Configurator);
    }

    private static TrainingEconomics PlankEconomics(
        CatalogueItem plank,
        decimal experiencePerPlank) =>
        new(
            [Input(plank, 1m / experiencePerPlank)],
            DemonButlerGpPerTrip / DemonButlerCapacity / experiencePerPlank);

    private static class Items
    {
        public static readonly CatalogueItem OakPlank = new(8778, "Oak plank");
        public static readonly CatalogueItem MahoganyPlank = new(8782, "Mahogany plank");
    }
}
