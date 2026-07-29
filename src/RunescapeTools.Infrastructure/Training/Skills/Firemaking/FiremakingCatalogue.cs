using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class FiremakingCatalogue
{
    private const decimal RosewoodLogBowFiremakingXp = 420m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Firemaking",
            Band(0, 73_700m, "Coloured logs"),
            Band(22_406, 138_900m, "Teak logs"),
            Band(45_529, 184_250m, "Arctic pine logs"),
            Band(61_512, 198_990m, "Maple logs"),
            Band(101_333, 400_271m, "Artefacts with firemaking"),
            Band(273_742, 522_696m, "Artefacts with firemaking"),
            Band(1_210_421, 768_800m, "Artefacts with firemaking"),
            Band(5_346_332, 864_981m, "Artefacts with firemaking"),
            Band(
                13_034_431,
                623_700m,
                "Rosewood logs - bow burning",
                new TrainingEconomics(
                    [Input(Items.RosewoodLogs, 1m / RosewoodLogBowFiremakingXp)])));

    private static class Items
    {
        public static readonly CatalogueItem RosewoodLogs = new(32910, "Rosewood logs");
    }
}
