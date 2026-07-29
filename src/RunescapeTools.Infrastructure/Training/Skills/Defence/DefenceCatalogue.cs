using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class DefenceCatalogue
{
    private const decimal ExperiencePerHour = 405_000m;
    private const decimal BlackChinchompasPerHour = 1_500m;
    private const decimal CannonballsPerHour = 6_000m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Defence",
            Band(
                0,
                ExperiencePerHour,
                "Black Chinchompas & Cannon - Defensive",
                new TrainingEconomics(
                    [
                        Input(
                            BlackChinchompa,
                            "Black chinchompa",
                            BlackChinchompasPerHour / ExperiencePerHour),
                        Input(
                            Cannonball,
                            "Cannonball",
                            CannonballsPerHour / ExperiencePerHour)
                    ])),
            "Reviewed projection at 405,000 Defence XP/hour. The cost model assumes 1,500 long-fuse " +
            "black chinchompas and maximum 6,000-cannonball throughput per hour.");
}
