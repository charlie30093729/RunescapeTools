using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Cooking.Methods;

internal static class OneTickKarambwans
{
    private const decimal ExperiencePerSuccessfulCook = 190m;
    private const decimal StandardAttemptsPerHour = 5_000m;

    public static TrainingMethodDefinition Create() =>
        new(
            "one-tick-karambwans",
            "1-tick karambwans",
            [
                Band(0, 172_800m, "1t poison karambwan"),
                CreateBand(13_363, 519_100m),
                CreateBand(37_224, 591_600m),
                CreateBand(101_333, 663_600m),
                CreateBand(273_742, 735_700m),
                CreateBand(737_627, 808_000m),
                CreateBand(1_986_068, 880_400m),
                CreateBand(5_346_332, 948_100m),
                CreateBand(13_034_431, 980_000m, assumeNoBurns: true)
            ],
            "Requires level 30 Cooking and completion of Tai Bwo Wannai Trio. Rates below level 99 " +
            "assume 5,000 one-tick cooking attempts per hour and include level-dependent burns. The " +
            "level-99 rate assumes no burns and optimized banking. Burnt karambwan have no output value.");

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        bool assumeNoBurns = false)
    {
        var rawKarambwanPerExperience = assumeNoBurns
            ? 1m / ExperiencePerSuccessfulCook
            : StandardAttemptsPerHour / experiencePerHour;

        return Band(
            startExperience,
            experiencePerHour,
            "1-tick karambwans",
            new TrainingEconomics(
            [
                Input(Items.RawKarambwan, rawKarambwanPerExperience),
                Output(Items.CookedKarambwan, 1m / ExperiencePerSuccessfulCook)
            ]));
    }

    private static class Items
    {
        public static readonly CatalogueItem RawKarambwan = new(3142, "Raw karambwan");
        public static readonly CatalogueItem CookedKarambwan = new(3144, "Cooked karambwan");
    }
}
