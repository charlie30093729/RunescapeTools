using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore.Methods;

internal static class ExtendedSuperAntifires
{
    private const decimal ExperiencePerPotion = 160m;

    public static TrainingMethodDefinition Create()
    {
        var methodBand = Band(
            11_805_606,
            840_000m,
            "1t extended super antifires",
            HerbloreGlobal.CreatePotionEconomics(
                Items.SuperAntifirePotion4,
                Items.LavaScaleShard,
                4m,
                Items.ExtendedSuperAntifire4,
                ExperiencePerPotion,
                baseOutputDosesPerPotion: 4m,
                prescriptionGogglesApply: true,
                alchemistsAmuletApplies: false));

        return new TrainingMethodDefinition(
            "1t-extended-super-antifires",
            "1t extended super antifires",
            HerbloreGlobal.CreateRoute(methodBand, SaradominBrews.CreateBand()),
            HerbloreGlobal.EquipmentNote);
    }

    private static class Items
    {
        public static readonly CatalogueItem SuperAntifirePotion4 =
            new(21978, "Super antifire potion(4)");
        public static readonly CatalogueItem LavaScaleShard = new(11994, "Lava scale shard");
        public static readonly CatalogueItem ExtendedSuperAntifire4 =
            new(22209, "Extended super antifire(4)");
    }
}
