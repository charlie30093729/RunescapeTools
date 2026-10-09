using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class HitpointsCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Hitpoints",
            Standalone("Combat training fallback", 90_000m),
            note: "Main EHP treats Hitpoints as zero-time combat XP; replace this editable fallback when session credits are enabled.");
}
