using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class RangedCatalogue
{
    private const decimal ExperiencePerHour = 1_150_000m;
    private const decimal BlackChinchompasPerHour = 1_866m;
    private const decimal CannonballsPerHour = 6_000m;

    public static TrainingSkillDefinition Create() =>
        new(
            "Ranged",
            [
                Band(
                    0,
                    ExperiencePerHour,
                    "Black Chinchompas & Cannon",
                    new TrainingEconomics(
                        [
                            Input(
                                Items.BlackChinchompa,
                                BlackChinchompasPerHour / ExperiencePerHour),
                            Input(
                                Items.Cannonball,
                                CannonballsPerHour / ExperiencePerHour)
                        ]))
            ],
            IsZeroTime: true,
            Note: "Ranged is priced at 1,150,000 XP/hour but contributes zero active hours. " +
                  "The cost model assumes 1,866 rapid black chinchompas and maximum " +
                  "6,000-cannonball throughput per hour.");

    private static class Items
    {
        public static readonly CatalogueItem BlackChinchompa = new(11959, "Black chinchompa");
        public static readonly CatalogueItem Cannonball = new(2, "Cannonball");
    }
}
