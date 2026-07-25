using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class ThievingCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Thieving",
            Band(0, 260_000m, "Gem knights"),
            "Reviewed planning projection: 260,000 XP/hour across the selected XP range. " +
            "Gem output, Tokkul, supplies, and account unlock requirements are not yet priced.");
}
