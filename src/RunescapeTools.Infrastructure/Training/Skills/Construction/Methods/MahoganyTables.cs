using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction.Methods;
internal static class MahoganyTables
{
    private const decimal ExperiencePerPlank = 140m;

    public static TrainingMethodDefinition Create()
    {
        var band = Band(
            123_000,
            940_000m,
            "Mahogany Tables",
            ConstructionGlobal.PlankEconomics(Items.MahoganyPlank, ExperiencePerPlank, 24m));
        return new TrainingMethodDefinition(
            "mahogany-tables",
            "Mahogany Tables",
            ConstructionGlobal.CreateRoute(band),
            "Requires level 52 construction. 940k is the efficient rate of mahogany tables.");
    }

    private static class Items
    {
        public static readonly CatalogueItem MahoganyPlank = new(8782, "Mahogany plank");
    }
}