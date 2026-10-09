using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Fishing.Methods;

internal static class MainEhp
{
    private const decimal ReviewedShardTotal = 4_894m;
    private const decimal ReviewedSeedTotal = 33m;
    private const long CrystalToolStartExperience = 814_445;

    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "2t swordfish and tuna",
            [
                Band(0, 29_200m, "Quests"),
                Band(14_612, 46_592m, "3t fly fishing"),
                Band(75_127, 84_686m, "Drift net fishing"),
                Band(106_046, 97_867m, "Drift net fishing"),
                Band(229_685, 112_877m, "Drift net fishing"),
                Band(302_288, 128_082m, "Drift net fishing"),
                Band(593_234, 139_313m, "Drift net fishing"),
                Band(737_627, 132_800m, "Drift net fishing"),
                Band(
                    CrystalToolStartExperience,
                    132_800m,
                    "2t swordfish and tuna - crystal harpoon",
                    new TrainingEconomics(
                    [
                        Input(
                            Items.EnhancedCrystalTeleportSeed,
                            ReviewedSeedTotal
                            / (TrainingPlanCalculator.MaximumExperience - CrystalToolStartExperience))
                    ]))
            ],
            $"Crystal charges use the reviewed all-skills 0-200m allocation of {ReviewedShardTotal:N0} shards " +
            $"({ReviewedSeedTotal:N0} whole enhanced seeds); fish are dropped.",
            UseStableDisplayName: true);

    private static class Items
    {
        public static readonly CatalogueItem EnhancedCrystalTeleportSeed =
            new(23959, "Enhanced crystal teleport seed (crystal harpoon charges)");
    }
}
