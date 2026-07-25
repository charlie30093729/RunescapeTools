using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class DefenceCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill("Defence", Band(0, 455_000m, "Black chinchompas and cannon - defensive"));
}
