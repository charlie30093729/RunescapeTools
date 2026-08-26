using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter.Methods;

internal static class AerialFishing
{
    private const long ReviewedRateStartExperience = 13_034_431;
    private const decimal HunterExperiencePerHour = 129_000m;
    private const decimal FishingExperiencePerHour = 99_500m;
    private const decimal BoostingPotionsPerHour = 2.5m;
    private const decimal PrayerPotionsPerHour = 2m;

    public static TrainingMethodDefinition Create() =>
        new(
            "aerial-fishing",
            "Aerial Fishing",
            MainEhp.Create().Bands
                .Where(band => band.StartExperience < ReviewedRateStartExperience)
                .Append(Band(
                    ReviewedRateStartExperience,
                    HunterExperiencePerHour,
                    "Aerial Fishing",
                    CreateEconomics(),
                    experienceOutputs:
                    [
                        new TrainingExperienceFlow(
                            "Fishing",
                            FishingExperiencePerHour / HunterExperiencePerHour)
                    ]))
                .ToArray(),
            "Aerial Fishing is accessible from level 35 Hunter and level 43 Fishing at Lake Molch, " +
            "but this reviewed boosted rate begins at level 99 in both skills. It earns 129,000 " +
            "Hunter and 99,500 Fishing XP/hour, uses Preserve, boosts to 105, and repots at 101: " +
            "one dose of super hunter and super fishing potion every six minutes, plus one prayer " +
            "potion(4) every 30 minutes. Enable Alry the Angler's whole-fish toggle and bring " +
            "initial bait or an eligible fish; when no appropriate bait remains, the cormorant " +
            "can eat caught fish directly, so no knife, fish-offcut processing, or recurring bait " +
            "cost is modelled. Cooking XP, Molch pearls, golden tench, and untradeable rewards are " +
            "excluded. " +
            "The default Hunter route is retained below level 99 because lower-level boosted rates " +
            "have not been reviewed.");

    private static TrainingEconomics CreateEconomics() =>
        new(
        [
            Input(Items.SuperHunterPotion4, 0m, BoostingPotionsPerHour),
            Input(Items.SuperFishingPotion4, 0m, BoostingPotionsPerHour),
            Input(Items.PrayerPotion4, 0m, PrayerPotionsPerHour)
        ]);

    private static class Items
    {
        public static readonly CatalogueItem SuperHunterPotion4 =
            new(31626, "Super hunter potion(4)");
        public static readonly CatalogueItem SuperFishingPotion4 =
            new(31602, "Super fishing potion(4)");
        public static readonly CatalogueItem PrayerPotion4 = new(2434, "Prayer potion(4)");
    }
}
