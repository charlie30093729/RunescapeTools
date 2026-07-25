using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class CookingCatalogue
{
    private const decimal SummerPieCookingXp = 260m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Cooking",
            Band(0, 172_800m, "1t poison karambwan"),
            Band(13_363, 519_100m, "1t karambwan"),
            Band(37_224, 591_600m, "1t karambwan"),
            Band(101_333, 663_600m, "1t karambwan"),
            Band(273_742, 735_700m, "1t karambwan"),
            Band(737_627, 808_000m, "1t karambwan"),
            Band(1_986_068, 880_400m, "1t karambwan"),
            Band(5_346_332, 948_100m, "1t karambwan"),
            Band(8_771_558, 490_000m, "Bake Pie spell - summer pies", SummerPieEconomics()));

    private static TrainingEconomics SummerPieEconomics() =>
        new(
            [
                Input(RawSummerPie, "Raw summer pie", 1m / SummerPieCookingXp),
                Input(AstralRune, "Astral rune", 1m / SummerPieCookingXp),
                Output(SummerPie, "Summer pie", 1m / SummerPieCookingXp)
            ]);
}
