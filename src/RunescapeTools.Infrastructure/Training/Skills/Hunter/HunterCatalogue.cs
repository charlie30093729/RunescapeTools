using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class HunterCatalogue
{
    private const decimal BlackChinchompaExperience = 315m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Hunter",
            "Black chinchompas are sold at the live low price after GE tax. PK/death losses and shooting-alt ammunition are excluded.",
            Band(0, 30_000m, "Varrock museum and birdhouses"),
            Band(2_107, 83_000m, "Oak birdhouses"),
            Band(7_028, 110_000m, "Willow birdhouses"),
            Band(20_224, 138_000m, "Teak birdhouses"),
            Band(55_649, 215_112m, "Drift net fishing"),
            Band(91_721, 268_770m, "Drift net fishing"),
            Band(184_040, 293_310m, "Drift net fishing"),
            Band(343_551, 322_424m, "Drift net fishing"),
            Band(737_627, 350_697m, "Drift net fishing"),
            Band(933_979, 275_000m, "Drift net fishing"),
            Band(
                992_895,
                265_000m,
                "Black chinchompas - shooting alt",
                new TrainingEconomics(
                    [Output(Items.BlackChinchompa, 1m / BlackChinchompaExperience)])));

    private static class Items
    {
        public static readonly CatalogueItem BlackChinchompa = new(11959, "Black chinchompa");
    }
}
