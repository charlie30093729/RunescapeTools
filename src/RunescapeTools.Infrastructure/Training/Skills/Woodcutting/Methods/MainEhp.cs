using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Woodcutting.Methods;

internal static class MainEhp
{
    private const decimal ReviewedRationTotal = 2_091_504m;
    private const decimal ReviewedShardTotal = 14_953m;
    private const decimal ReviewedSeedTotal = 100m;
    private const long TeakStartExperience = 22_406;
    private const long CrystalToolStartExperience = 814_445;

    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "1.5t teaks",
            [
                Band(0, 29_000m, "Quests and trees"),
                Band(2_411, 56_000m, "2t oaks"),
                Band(22_406, 93_174m, "1.5t teaks", TeakEconomics()),
                Band(41_171, 114_728m, "1.5t teaks", TeakEconomics()),
                Band(111_945, 127_339m, "1.5t teaks", TeakEconomics()),
                Band(302_288, 172_507m, "1.5t teaks", TeakEconomics()),
                Band(814_445, 194_022m, "1.5t teaks - crystal felling axe", TeakEconomics(includeCrystalCharges: true)),
                Band(1_986_068, 207_636m, "1.5t teaks - crystal felling axe", TeakEconomics(includeCrystalCharges: true)),
                Band(5_346_332, 221_977m, "1.5t teaks - crystal felling axe", TeakEconomics(includeCrystalCharges: true)),
                Band(13_034_431, 235_000m, "1.5t teaks - crystal felling axe", TeakEconomics(includeCrystalCharges: true))
            ],
            $"Reviewed 0-200m resources: {ReviewedRationTotal:N0} Forester's rations, " +
            $"{ReviewedShardTotal:N0} crystal shards ({ReviewedSeedTotal:N0} whole enhanced seeds). Teak logs are dropped.",
            UseStableDisplayName: true);

    private static TrainingEconomics TeakEconomics(bool includeCrystalCharges = false)
    {
        var resources = new List<TrainingResourceFlow>
        {
            Input(
                Items.ForestersRation,
                ReviewedRationTotal / (TrainingPlanCalculator.MaximumExperience - TeakStartExperience))
        };
        if (includeCrystalCharges)
        {
            resources.Add(Input(
                Items.EnhancedCrystalTeleportSeed,
                ReviewedSeedTotal / (TrainingPlanCalculator.MaximumExperience - CrystalToolStartExperience)));
        }

        return new TrainingEconomics(resources);
    }

    private static class Items
    {
        public static readonly CatalogueItem ForestersRation = new(28157, "Forester's ration");
        public static readonly CatalogueItem EnhancedCrystalTeleportSeed =
            new(23959, "Enhanced crystal teleport seed (crystal felling axe charges)");
    }
}
