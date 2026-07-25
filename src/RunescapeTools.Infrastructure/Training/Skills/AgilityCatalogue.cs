using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class AgilityCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Agility",
            "Hallowed Sepulchre rates include Agility experience only; looting and multiskilling are excluded.",
            Band(0, 15_100m, "Quests"),
            Band(75_127, 35_000m, "Wilderness Agility Course"),
            Band(123_660, 40_000m, "Hallowed Sepulchre - no looting"),
            Band(333_804, 50_000m, "Hallowed Sepulchre - no looting"),
            Band(899_257, 71_700m, "Hallowed Sepulchre - no looting"),
            Band(2_421_087, 81_000m, "Hallowed Sepulchre - no looting"),
            Band(6_517_253, 98_500m, "Hallowed Sepulchre - no looting"));
}
