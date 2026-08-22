using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Woodcutting;

internal static class WoodcuttingGlobal
{
    // One enhanced crystal teleport seed yields 150 crystal shards, and each shard adds
    // 100 charges. A regular crystal axe consumes one charge per successfully cut log.
    private const decimal CrystalAxeChargesPerEnhancedSeed = 15_000m;

    private static readonly CatalogueItem EnhancedCrystalTeleportSeed =
        new(23959, "Enhanced crystal teleport seed (crystal axe charges)");

    public static TrainingEconomics CreateBankedLogEconomics(
        CatalogueItem log,
        decimal experiencePerLog) =>
        new(
        [
            Input(
                EnhancedCrystalTeleportSeed,
                1m / (experiencePerLog * CrystalAxeChargesPerEnhancedSeed)),
            Output(log, 1m / experiencePerLog)
        ]);
}
