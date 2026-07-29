using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class ConstructionCatalogue
{
    private const decimal DemonButlerGpPerTrip = 10_000m / 8m;
    private const decimal DemonButlerCapacity = 24m;

    public static TrainingSkillDefinition Create()
    {
        var oakEconomics = PlankEconomics(OakPlank, "Oak plank", 60m);
        var mahoganyEconomics = PlankEconomics(MahoganyPlank, "Mahogany plank", 140m);
        return Skill(
            "Construction",
            Band(0, 54_700m, "Low-level furniture"),
            Band(18_247, 200_000m, "Oak larders", oakEconomics),
            Band(37_224, 290_000m, "Mahogany bookcases", mahoganyEconomics),
            Band(123_660, 950_000m, "Mahogany tables", mahoganyEconomics),
            Band(1_475_581, 1_070_000m, "Mahogany benches", mahoganyEconomics),
            Band(13_034_431, 1_440_000m, "2t mahogany flatpacks", mahoganyEconomics));
    }

    private static TrainingEconomics PlankEconomics(
        int itemId,
        string name,
        decimal experiencePerPlank) =>
        new(
            [Input(itemId, name, 1m / experiencePerPlank)],
            DemonButlerGpPerTrip / DemonButlerCapacity / experiencePerPlank);
}
