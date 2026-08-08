using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter.Methods;

internal static class RedChinchompas
{
    private const long UnlockExperience = 368_599;
    private const decimal ExperiencePerCatch = 265m;

    public static TrainingMethodDefinition Create() =>
        new(
            "red-chinchompas",
            "Red chinchompas",
            [
                .. MainEhp.Create().Bands.Where(band => band.StartExperience < UnlockExperience),
                CreateBand(UnlockExperience, 70_000m),
                CreateBand(737_627, 93_000m),
                CreateBand(1_986_068, 143_900m),
                CreateBand(5_346_332, 171_200m),
                CreateBand(13_034_431, 210_000m)
            ],
            "Requires level 63 Hunter and partial completion of Eagles' Peak. Rates assume tick " +
            "manipulation at a dense red-chinchompa area and scale with Hunter level; the level-99 " +
            "rate assumes maximum-efficiency play with a shooting alt. Every successful catch is " +
            "valued at the live low price after GE tax. Consumable tick-manipulation items and " +
            "shooting-alt supplies are excluded.");

    private static TrainingRateBand CreateBand(long startExperience, decimal experiencePerHour) =>
        Band(
            startExperience,
            experiencePerHour,
            "Red chinchompas",
            new TrainingEconomics(
            [
                Output(Items.RedChinchompa, 1m / ExperiencePerCatch)
            ]));

    private static class Items
    {
        public static readonly CatalogueItem RedChinchompa = new(10034, "Red chinchompa");
    }
}
