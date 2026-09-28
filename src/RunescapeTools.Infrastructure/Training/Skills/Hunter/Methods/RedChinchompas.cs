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
                .. BlackChinchompas.Create().Bands.Where(band => band.StartExperience < UnlockExperience),
                CreateBand(UnlockExperience, 70_000m),
                CreateBand(737_627, 93_000m),
                CreateBand(1_986_068, 143_900m),
                CreateBand(5_346_332, 171_200m),
                CreateBand(13_034_431, 210_000m)
            ],
            "Requires level 63 Hunter and partial completion of Eagles' Peak. 3-ticking is on by " +
            "default, preserving the reviewed 70k/93k/143.9k/171.2k/210k presets at levels " +
            "63/70/80/90/99. With 3-ticking off, use 61k/72k/115k/136k/170k respectively. " +
            "The 170k level-99 non-tick rate is the user's planning assumption; lower non-tick " +
            "bands use the Wiki's dense Tlati-area estimates, held until the next band. " +
            "Location access and player execution affect actual rates; this is not an AFK method. " +
            "No shooting-alt setting applies to red chins. Personal rates scale relative to the " +
            "configured defaults; XP/catch and quantities per goal stay fixed. Every catch sells " +
            "low after GE tax. Tick-manipulation consumables and setup/travel are excluded.",
            UseStableDisplayName: true);

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
