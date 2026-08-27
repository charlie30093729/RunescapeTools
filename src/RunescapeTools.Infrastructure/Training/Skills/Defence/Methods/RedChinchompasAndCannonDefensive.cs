using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Defence.Methods;

internal static class RedChinchompasAndCannonDefensive
{
    private const decimal ExperiencePerHour = 330_000m;
    private const decimal ChinchompasPerHour = 1_500m;
    private const decimal CannonballsPerHour = 6_000m;

    public static TrainingMethodDefinition Create() =>
        new(
            "red-chinchompas-cannon-defensive",
            "Red Chinchompas & Cannon - Defensive",
            [CreateBand()],
            "Requires level 55 Ranged and uses long fuse to train Defence. The reviewed 330,000 " +
            "Defence XP/hour projection applies the current level-99 red-to-black chinchompa " +
            "throughput ratio to the existing defensive route. It assumes 1,500 red " +
            "chinchompas and maximum 6,000-cannonball throughput per hour; the rate can be " +
            "overridden per profile.");

    private static TrainingRateBand CreateBand() =>
        Band(
            0,
            ExperiencePerHour,
            "Red Chinchompas & Cannon - Defensive",
            new TrainingEconomics(
            [
                Input(Items.RedChinchompa, ChinchompasPerHour / ExperiencePerHour),
                Input(Items.Cannonball, CannonballsPerHour / ExperiencePerHour)
            ]));

    private static class Items
    {
        public static readonly CatalogueItem RedChinchompa = new(10034, "Red chinchompa");
        public static readonly CatalogueItem Cannonball = new(2, "Cannonball");
    }
}
