using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Mining.Methods;

internal static class CalcifiedRocks
{
    private const string DisplayName = "Calcified rocks - crystal pickaxe";
    private const decimal DepositChance = 1m / 75m;
    private const decimal BoneShardRollChance = 74m / 75m;
    private const decimal AverageShardsPerShardRoll = 2m;
    // A successful main-resource roll awards 33 XP, or 36 when it yields a deposit.
    // MiningGlobal applies the default-on Prospector bonus to rates and per-XP flows.
    private const decimal ExperiencePerSuccess = 33m + 3m * DepositChance;

    public static TrainingMethodDefinition Create() => new(
        "calcified-rocks-crystal-pickaxe", DisplayName,
        MiningGlobal.WithMainRouteBeforeUnlock(
        [
            CreateBand(MiningGlobal.CrystalPickaxeUnlockExperience, 34_000m),
            CreateBand(1_986_068, 39_000m), // 80
            CreateBand(5_346_332, 44_000m), // 90
            CreateBand(13_034_431, 49_000m) // 99+
        ]),
        "Low-attention mining, without tick manipulation. This crystal-pickaxe variant starts at " +
        "71 Mining and requires Song of the Elves plus access to Cam Torum through Perilous Moons. " +
        "The rocks themselves unlock at 41; the existing route is retained until crystal use at 71. " +
        "Rates use the Wiki's non-tick-manipulation planning estimates: 34k at 71-79, 39k at 80, " +
        "44k at 90 and 49k at 99. Actual rates depend on attention and watery veins; the level-70 " +
        "estimate is carried forward to 71, without an additional assumed crystal-speed multiplier. " +
        "Costs estimate one crystal charge per successful main-resource roll, buying enhanced " +
        "teleport seeds for 150 shards/15,000 charges each. Full Prospector is enabled by default, " +
        "applying a 2.5% bonus to these base rates and reducing materials per XP. No signet charge saving " +
        "or starting charge stock is assumed. Tool purchase/creation, travel and rare gem rolls are excluded. " +
        "Blessed bone shards and unopened calcified deposits are retained as untradeable outputs, " +
        "not GP or automatic Prayer XP. Deposit processing, moth sales and bone-shard offering " +
        "time/supplies are separate activities and are not included.",
        UseStableDisplayName: true);

    private static TrainingRateBand CreateBand(long startExperience, decimal rate) =>
        Band(startExperience, rate, DisplayName, new TrainingEconomics(
        [
            Input(Items.EnhancedCrystalTeleportSeed,
                1m / (ExperiencePerSuccess * MiningGlobal.CrystalChargesPerEnhancedSeed)),
            new TrainingResourceFlow(Items.BlessedBoneShards.Id, Items.BlessedBoneShards.Name,
                BoneShardRollChance * AverageShardsPerShardRoll / ExperiencePerSuccess,
                TrainingFlowDirection.Output, SubjectToGeTax: false, RequiresMarketPrice: false),
            new TrainingResourceFlow(Items.CalcifiedDeposit.Id, Items.CalcifiedDeposit.Name,
                DepositChance / ExperiencePerSuccess,
                TrainingFlowDirection.Output, SubjectToGeTax: false, RequiresMarketPrice: false)
        ]));

    private static class Items
    {
        public static readonly CatalogueItem EnhancedCrystalTeleportSeed =
            new(23959, "Enhanced crystal teleport seed (crystal pickaxe charges)");
        public static readonly CatalogueItem BlessedBoneShards = new(29381, "Blessed bone shards");
        public static readonly CatalogueItem CalcifiedDeposit = new(29088, "Calcified deposit");
    }
}
