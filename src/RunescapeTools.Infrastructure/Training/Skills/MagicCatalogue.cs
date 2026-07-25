using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class MagicCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Magic",
            Standalone("Ice Barrage fallback", 330_000m),
            note: "Main EHP treats Magic as zero-time. The rate is editable until the standalone method is fully reviewed.");
}
