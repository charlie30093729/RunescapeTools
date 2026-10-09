using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Defence.Methods;

internal static class BlackChinchompasAndCannonDefensive
{
    private const decimal ExperiencePerHour = 405_000m;
    private const decimal ChinchompasPerHour = 1_500m;
    private const decimal CannonballsPerHour = 6_000m;

    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "Black Chinchompas & Cannon - Defensive",
            [CreateBand()],
            "Reviewed projection at 405,000 Defence XP/hour. The cost model assumes 1,500 " +
            "long-fuse black chinchompas and maximum 6,000-cannonball throughput per hour.");

    private static TrainingRateBand CreateBand() =>
        Band(
            0,
            ExperiencePerHour,
            "Black Chinchompas & Cannon - Defensive",
            new TrainingEconomics(
            [
                Input(Items.BlackChinchompa, ChinchompasPerHour / ExperiencePerHour),
                Input(Items.Cannonball, CannonballsPerHour / ExperiencePerHour)
            ]));

    private static class Items
    {
        public static readonly CatalogueItem BlackChinchompa = new(11959, "Black chinchompa");
        public static readonly CatalogueItem Cannonball = new(2, "Cannonball");
    }
}
