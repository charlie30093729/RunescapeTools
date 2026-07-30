using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore.Methods;

internal static class SaradominBrews
{
    private const decimal ExperiencePerPotion = 180m;

    public static TrainingMethodDefinition Create(HerbloreGlobal.HerbloreSettings settings)
    {
        var methodBand = CreateBand(settings);

        return new TrainingMethodDefinition(
            "main-ehp",
            "Saradomin brews",
            HerbloreGlobal.CreateRoute(methodBand),
            HerbloreGlobal.EquipmentNote);
    }

    internal static TrainingRateBand CreateBand(HerbloreGlobal.HerbloreSettings settings) =>
        Band(
            2_192_818,
            450_000m,
            "Saradomin brews",
            HerbloreGlobal.CreatePotionEconomics(
                Items.ToadflaxPotionUnfinished,
                Items.CrushedNest,
                1m,
                Items.SaradominBrew4,
                ExperiencePerPotion,
                settings));

    private static class Items
    {
        public static readonly CatalogueItem ToadflaxPotionUnfinished =
            new(3002, "Toadflax potion (unf)");
        public static readonly CatalogueItem CrushedNest = new(6693, "Crushed nest");
        public static readonly CatalogueItem SaradominBrew4 = new(6685, "Saradomin brew(4)");
    }
}
