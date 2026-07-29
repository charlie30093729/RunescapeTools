using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore.Methods;

internal static class SuperRestores
{
    private const decimal ExperiencePerPotion = 142.5m;

    public static TrainingMethodDefinition Create()
    {
        var methodBand = Band(
            368_599,
            356_250m,
            "Super restores",
            HerbloreGlobal.CreatePotionEconomics(
                Items.SnapdragonPotionUnfinished,
                Items.RedSpidersEggs,
                1m,
                Items.SuperRestore4,
                ExperiencePerPotion));

        return new TrainingMethodDefinition(
            "super-restores",
            "Super restores",
            HerbloreGlobal.CreateRoute(methodBand),
            HerbloreGlobal.EquipmentNote);
    }

    private static class Items
    {
        public static readonly CatalogueItem SnapdragonPotionUnfinished =
            new(3004, "Snapdragon potion (unf)");
        public static readonly CatalogueItem RedSpidersEggs = new(223, "Red spiders' eggs");
        public static readonly CatalogueItem SuperRestore4 = new(3024, "Super restore(4)");
    }
}
