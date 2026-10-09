using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Ranged.Methods;

internal static class RedChinchompasAndCannon
{
    private const decimal ExperiencePerHour = 940_000m;
    private const decimal ChinchompasPerHour = 1_866m;
    private const decimal CannonballsPerHour = 6_000m;

    public static TrainingMethodDefinition Create() =>
        new(
            "red-chinchompas-cannon",
            "Red Chinchompas & Cannon",
            [CreateBand()],
            "Requires level 55 Ranged and access to an efficient stacked target. The reviewed " +
            "940,000 XP/hour projection applies the current level-99 red-to-black chinchompa " +
            "throughput ratio to the existing cannon-assisted route. It assumes 1,866 rapid red " +
            "chinchompas and maximum 6,000-cannonball throughput per calculation hour. Ranged " +
            "remains zero-time in the planner, and the rate can be overridden per profile.");

    private static TrainingRateBand CreateBand() =>
        Band(
            0,
            ExperiencePerHour,
            "Red Chinchompas & Cannon",
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
