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
            "Ironwood trees - crystal felling axe",
            MainEhp.Create().Bands
                .Where(band => band.StartExperience < UnlockExperience)
                .Concat(
                [
                    CreateBand(UnlockExperience, 82_500m),
                    CreateBand(5_346_332, 93_500m),
                    CreateBand(13_034_431, 104_500m)
                ])
                .ToArray(),
            "Requires level 80 Woodcutting and either 72 Sailing for Sunbleak Island or level 80 " +
            "Farming for a private tree. Rates apply the crystal felling axe's 10% XP bonus to " +
            "the regular-crystal-axe planning curve. One Forester's ration is consumed per " +
            "successful chop; 20% of chops award no log. Received ironwood logs are banked and " +
            "sold, and crystal charges are priced.");

    private static TrainingRateBand CreateBand(long startExperience, decimal experiencePerHour) =>
        Band(
            startExperience,
            experiencePerHour,
            "Ironwood trees - crystal felling axe",
            WoodcuttingGlobal.CreateFellingAxeEconomics(Items.IronwoodLogs, ExperiencePerLog));

    private static class Items
    {
        public static readonly CatalogueItem IronwoodLogs = new(32907, "Ironwood logs");
    }
}
