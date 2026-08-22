using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Woodcutting.Methods;

internal static class IronwoodTrees
{
    private const long UnlockExperience = 1_986_068;
    private const decimal ExperiencePerLog = 175m;

    public static TrainingMethodDefinition Create() =>
        new(
            "ironwood-trees-crystal-axe",
            "Ironwood trees - crystal axe",
            MainEhp.Create().Bands
                .Where(band => band.StartExperience < UnlockExperience)
                .Concat(
                [
                    CreateBand(UnlockExperience, 75_000m),
                    CreateBand(5_346_332, 85_000m),
                    CreateBand(13_034_431, 95_000m)
                ])
                .ToArray(),
            "Requires level 80 Woodcutting and either 72 Sailing for Sunbleak Island or level 80 " +
            "Farming for a private tree. Rates apply the crystal axe's 4.5% advantage to the " +
            "documented dragon-axe range and are rounded to readable planning bands. They " +
            "exclude the felling axe and Forester's rations. Ironwood logs are banked and sold; " +
            "crystal axe charges are priced.");

    private static TrainingRateBand CreateBand(long startExperience, decimal experiencePerHour) =>
        Band(
            startExperience,
            experiencePerHour,
            "Ironwood trees - crystal axe",
            WoodcuttingGlobal.CreateBankedLogEconomics(Items.IronwoodLogs, ExperiencePerLog));

    private static class Items
    {
        public static readonly CatalogueItem IronwoodLogs = new(32907, "Ironwood logs");
    }
}
