using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore.Methods;

internal static class SaradominBrews
{
    private const decimal ExperiencePerPotion = 180m;

    public static TrainingMethodDefinition Create()
    {
        var methodBand = CreateBand();

        return new TrainingMethodDefinition(
            "main-ehp",
            "Saradomin brews",
            HerbloreGlobal.CreateRoute(methodBand),
            HerbloreGlobal.EquipmentNote);
    }

    internal static TrainingRateBand CreateBand() =>
        Band(
            2_192_818,
            450_000m,
            "Saradomin brews",
            HerbloreGlobal.CreatePotionEconomics(
                ToadflaxPotionUnfinished,
                "Toadflax potion (unf)",
                CrushedNest,
                "Crushed nest",
                1m,
                SaradominBrew4,
                "Saradomin brew(4)",
                ExperiencePerPotion));
}
