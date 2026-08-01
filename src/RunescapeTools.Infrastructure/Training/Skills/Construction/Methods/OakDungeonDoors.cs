using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction.Methods;

internal static class OakDungeonDoors
{
    private const decimal ExperiencePerPlank = 60m;

    public static TrainingMethodDefinition Create()
    {
        var band = Band(
            1_210_421,
            550_000m,
            "Oak dungeon doors",
            ConstructionGlobal.PlankEconomics(Items.OakPlank, ExperiencePerPlank, 25m));
        return new TrainingMethodDefinition(
            "oak-dungeon-doors",
            "Oak dungeon doors",
            ConstructionGlobal.CreateRoute(band),
            "Requires level 74 Construction. Each door uses 10 oak planks for 600 XP; servant fees assume the documented 25-plank Demon butler cycle.");
    }

    private static class Items
    {
        public static readonly CatalogueItem OakPlank = new(8778, "Oak plank");
    }
}
