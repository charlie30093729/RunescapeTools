using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class SailingCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Sailing",
            Band(0, 27_000m, "Quests, Tears of Guthix and Tempor Tantrum"),
            Band(101_333, 45_000m, "1.5t large shipwrecks"),
            Band(166_636, 100_000m, "The Jubbly Jive and charting"),
            Band(899_257, 220_000m, "The Gwenith Glide - camphor hull"),
            Band(4_842_295, 255_000m, "The Gwenith Glide - rosewood hull with Spin Flax"));
}
