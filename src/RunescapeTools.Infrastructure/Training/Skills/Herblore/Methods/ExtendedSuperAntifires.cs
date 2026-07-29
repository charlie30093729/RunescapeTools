using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

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
                SuperAntifirePotion4,
                "Super antifire potion(4)",
                LavaScaleShard,
                "Lava scale shard",
                4m,
                ExtendedSuperAntifire4,
                "Extended super antifire(4)",
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
}
