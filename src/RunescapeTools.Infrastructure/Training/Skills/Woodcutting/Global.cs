using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Woodcutting;

internal static class WoodcuttingGlobal
{
    public const decimal FellingAxeExperienceMultiplier = 1.10m;
    private const decimal FellingAxeLogReceiptChance = 0.80m;

    // One enhanced crystal teleport seed yields 150 crystal shards, and each shard adds
    // 100 charges. Crystal tools consume one charge only when an item is obtained.
    private const decimal CrystalAxeChargesPerEnhancedSeed = 15_000m;

    private static readonly CatalogueItem EnhancedCrystalTeleportSeed =
        new(23959, "Enhanced crystal teleport seed (crystal felling axe charges)");
    private static readonly CatalogueItem ForestersRation = new(28157, "Forester's ration");

    public static TrainingEconomics CreateFellingAxeEconomics(
        CatalogueItem log,
        decimal baseExperiencePerChop)
    {
        var experiencePerChop = baseExperiencePerChop * FellingAxeExperienceMultiplier;
        return new TrainingEconomics(
        [
            Input(
                EnhancedCrystalTeleportSeed,
                FellingAxeLogReceiptChance / (experiencePerChop * CrystalAxeChargesPerEnhancedSeed)),
            Input(ForestersRation, 1m / experiencePerChop),
            Output(log, FellingAxeLogReceiptChance / experiencePerChop)
        ]);
    }
}
