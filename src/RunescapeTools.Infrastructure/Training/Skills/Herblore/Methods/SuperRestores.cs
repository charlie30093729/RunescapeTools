using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

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
                SnapdragonPotionUnfinished,
                "Snapdragon potion (unf)",
                RedSpidersEggs,
                "Red spiders' eggs",
                1m,
                SuperRestore4,
                "Super restore(4)",
                ExperiencePerPotion));

        return new TrainingMethodDefinition(
            "super-restores",
            "Super restores",
            HerbloreGlobal.CreateRoute(methodBand),
            HerbloreGlobal.EquipmentNote);
    }
}
