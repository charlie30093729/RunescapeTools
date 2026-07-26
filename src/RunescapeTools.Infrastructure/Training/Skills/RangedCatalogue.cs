using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

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
                                BlackChinchompa,
                                "Black chinchompa",
                                BlackChinchompasPerHour / ExperiencePerHour),
                            Input(
                                Cannonball,
                                "Cannonball",
                                CannonballsPerHour / ExperiencePerHour)
                        ]))
            ],
            IsZeroTime: true,
            Note: "Ranged is priced at 1,150,000 XP/hour but contributes zero active hours. " +
                  "The cost model assumes 1,866 rapid black chinchompas and maximum " +
                  "6,000-cannonball throughput per hour.");
}
