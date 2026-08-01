using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Crafting.Methods;

internal static class AirBattlestaves
{
    private const decimal ExperiencePerStaff = 137.5m;

    public static TrainingMethodDefinition Create()
    {
        var band = Band(
            496_254,
            336_875m,
            "Air battlestaves",
            new TrainingEconomics(
            [
                Input(Items.Battlestaff, 1m / ExperiencePerStaff),
                Input(Items.AirOrb, 1m / ExperiencePerStaff),
                Output(Items.AirBattlestaff, 1m / ExperiencePerStaff)
            ]));
        return new TrainingMethodDefinition(
            "air-battlestaves",
            "Air battlestaves",
            CraftingGlobal.CreateRoute(band),
            "Requires level 66 Crafting. The reviewed rate assumes 2,450 battlestaves crafted per hour.");
    }

    private static class Items
    {
        public static readonly CatalogueItem Battlestaff = new(1391, "Battlestaff");
        public static readonly CatalogueItem AirOrb = new(573, "Air orb");
        public static readonly CatalogueItem AirBattlestaff = new(1397, "Air battlestaff");
    }
}
