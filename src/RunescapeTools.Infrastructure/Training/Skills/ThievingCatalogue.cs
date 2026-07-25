using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class ThievingCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Thieving",
            Band(0, 15_000m, "Quests and fruit stalls"),
            Band(61_512, 80_000m, "Blackjacking"),
            Band(91_721, 247_014m, "Artefacts with firemaking"),
            Band(295_921, 291_617m, "Artefacts with firemaking"),
            Band(1_322_779, 340_358m, "Artefacts with firemaking"),
            Band(4_814_243, 378_482m, "Artefacts with firemaking"),
            Band(10_999_977, 374_790m, "Artefacts with Bake Pie"),
            Band(13_034_431, 381_266m, "Artefacts with Bake Pie"));
}
