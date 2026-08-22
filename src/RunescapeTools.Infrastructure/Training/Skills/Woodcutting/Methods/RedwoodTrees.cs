using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Woodcutting.Methods;

internal static class RedwoodTrees
{
    private const long UnlockExperience = 5_346_332;
    private const decimal ExperiencePerLog = 380m;

    public static TrainingMethodDefinition Create() =>
        new(
            "redwood-trees-crystal-axe",
            "Redwood trees - crystal axe",
            MainEhp.Create().Bands
                .Where(band => band.StartExperience < UnlockExperience)
                .Concat(
                [
                    CreateBand(UnlockExperience, 70_000m),
                    CreateBand(13_034_431, 75_000m)
                ])
                .ToArray(),
            "Requires level 90 Woodcutting and access to the Woodcutting Guild. Rates follow the " +
            "current Wiki range for a regular crystal axe, without a felling axe or Forester's " +
            "rations. Redwood logs are banked and sold; crystal axe charges are priced.");

    private static TrainingRateBand CreateBand(long startExperience, decimal experiencePerHour) =>
        Band(
            startExperience,
            experiencePerHour,
            "Redwood trees - crystal axe",
            WoodcuttingGlobal.CreateBankedLogEconomics(Items.RedwoodLogs, ExperiencePerLog));

    private static class Items
    {
        public static readonly CatalogueItem RedwoodLogs = new(19669, "Redwood logs");
    }
}
