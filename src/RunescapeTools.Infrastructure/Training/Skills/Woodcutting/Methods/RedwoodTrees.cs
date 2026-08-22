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
            "Redwood trees - crystal felling axe",
            MainEhp.Create().Bands
                .Where(band => band.StartExperience < UnlockExperience)
                .Concat(
                [
                    CreateBand(UnlockExperience, 77_000m),
                    CreateBand(13_034_431, 82_500m)
                ])
                .ToArray(),
            "Requires level 90 Woodcutting, access to the Woodcutting Guild, and Forester's " +
            "rations. Rates apply the crystal felling axe's 10% XP bonus to the current Wiki " +
            "crystal-axe range. One ration is consumed per successful chop; 20% of chops award " +
            "no log. Received redwood logs are banked and sold, and crystal charges are priced.",
            UseStableDisplayName: true);

    private static TrainingRateBand CreateBand(long startExperience, decimal experiencePerHour) =>
        Band(
            startExperience,
            experiencePerHour,
            "Redwood trees - crystal felling axe",
            WoodcuttingGlobal.CreateFellingAxeEconomics(Items.RedwoodLogs, ExperiencePerLog));

    private static class Items
    {
        public static readonly CatalogueItem RedwoodLogs = new(19669, "Redwood logs");
    }
}
