using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction.Methods;

internal static class MainEhp
{
    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "Mahogany furniture",
            ConstructionGlobal.CreateRoute(
                Band(
                    13_034_431,
                    1_440_000m,
                    "2t mahogany flatpacks",
                    ConstructionGlobal.PlankEconomics(Items.MahoganyPlank, 140m, 24m))));

    internal static class Items
    {
        public static readonly CatalogueItem OakPlank = new(8778, "Oak plank");
        public static readonly CatalogueItem MahoganyPlank = new(8782, "Mahogany plank");
    }
}
