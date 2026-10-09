using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Cooking.Methods;

internal static class MainEhp
{
    private const decimal SummerPieCookingXp = 260m;

    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "Main EHP route",
            [
                Band(0, 172_800m, "1t poison karambwan"),
                Band(13_363, 519_100m, "1t karambwan"),
                Band(37_224, 591_600m, "1t karambwan"),
                Band(101_333, 663_600m, "1t karambwan"),
                Band(273_742, 735_700m, "1t karambwan"),
                Band(737_627, 808_000m, "1t karambwan"),
                Band(1_986_068, 880_400m, "1t karambwan"),
                Band(5_346_332, 948_100m, "1t karambwan"),
                Band(8_771_558, 490_000m, "Bake Pie spell - summer pies", SummerPieEconomics())
            ]);

    private static TrainingEconomics SummerPieEconomics() =>
        new(
        [
            Input(Items.RawSummerPie, 1m / SummerPieCookingXp),
            Input(Items.AstralRune, 1m / SummerPieCookingXp),
            Output(Items.SummerPie, 1m / SummerPieCookingXp)
        ]);

    private static class Items
    {
        public static readonly CatalogueItem RawSummerPie = new(7216, "Raw summer pie");
        public static readonly CatalogueItem AstralRune = new(9075, "Astral rune");
        public static readonly CatalogueItem SummerPie = new(7218, "Summer pie");
    }
}
