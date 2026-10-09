using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class SlayerCatalogue
{
    private const decimal ReviewedExperiencePerHour = 123_040m;
    private const decimal ReviewedTasks = 6_578m;
    private const decimal SlayerExperiencePerTask = 28_397m;
    private const decimal ReviewedMagicExperience = 163_136_972m;
    private const decimal MagicExperiencePerSlayerExperience =
        ReviewedMagicExperience / (ReviewedTasks * SlayerExperiencePerTask);

    public static TrainingSkillDefinition Create() =>
        new(
            "Slayer",
            [
                Band(
                    0,
                    ReviewedExperiencePerHour,
                    "Efficient Slayer task list",
                    new TrainingEconomics([]))
            ],
            Note: "Reviewed task-list projection: 123,040 Slayer XP/hour. Only Magic receives deferred " +
                  "secondary XP; Attack, Strength, Defence, Hitpoints, Ranged, and Prayer credits are excluded. " +
                  "Slayer supplies and loot are treated as break-even at an explicit 0 GP/XP.",
            ExperienceOutputs:
            [
                new TrainingExperienceFlow("Magic", MagicExperiencePerSlayerExperience)
            ]);
}
