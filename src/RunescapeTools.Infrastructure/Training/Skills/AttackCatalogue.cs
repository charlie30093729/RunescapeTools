using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class AttackCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Attack",
            Standalone("Nightmare Zone fallback", 115_000m),
            note: "Main EHP treats Attack as Slayer bonus XP. This editable standalone fallback keeps Slayer isolated.");
}
