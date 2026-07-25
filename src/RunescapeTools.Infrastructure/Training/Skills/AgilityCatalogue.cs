using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class AgilityCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Agility",
            Band(0, 15_100m, "Quests"),
            Band(75_127, 35_000m, "Wilderness Agility Course"),
            Band(123_660, 45_000m, "Hallowed Sepulchre"),
            Band(333_804, 56_300m, "Hallowed Sepulchre"),
            Band(899_257, 68_900m, "Hallowed Sepulchre"),
            Band(2_421_087, 79_700m, "Hallowed Sepulchre"),
            Band(6_517_253, 102_000m, "Hallowed Sepulchre with brews"));
}
