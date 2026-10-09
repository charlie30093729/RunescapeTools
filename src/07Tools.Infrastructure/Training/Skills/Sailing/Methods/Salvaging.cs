using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Sailing.Methods;

internal static class Salvaging
{
    public static TrainingMethodDefinition Create(bool useCrystalExtractor = true)
    {
        // Planning estimates from the Wiki's active salvaging table, not a per-level
        // success-chance simulation. Upgrade hooks/boat/crew only after their unlocks.
        TrainingRateBand[] bands =
        [
            // Retain the application's existing early-level planning placeholder;
            // this is NOT salvaging or a claim that Gwenith is playable below level 15.
            GwenithGlide.Create().Bands[0],
            Band(2_411, 2_800m, "Small shipwreck - bronze hook"),                  // 15
            Band(5_018, 3_200m, "Small shipwreck - iron hook"),                    // 21
            Band(9_730, 5_800m, "Fisherman's shipwreck - steel hook"),              // 27
            Band(22_406, 11_000m, "Barracuda shipwreck - steel hook"),              // 35
            Band(55_649, 14_500m, "Barracuda shipwreck - mithril hook"),            // 44
            Band(101_333, 16_000m, "Barracuda shipwreck - two mithril hooks"),      // 50, sloop
            Band(136_594, 24_000m, "Large shipwreck - two mithril hooks"),          // 53
            Band(247_886, 25_000m, "Large shipwreck - two adamant hooks"),          // 59
            Band(273_742, 30_000m, "Large shipwreck - adamant hooks, Jenkins"),     // 60
            Band(407_015, 47_000m, "Pirate shipwreck - two adamant hooks"),         // 64
            Band(992_895, 60_000m, "Mercenary shipwreck - two adamant hooks"),      // 73
            Band(1_096_278, 70_000m, "Mercenary shipwreck - two rune hooks"),       // 74
            Band(1_986_068, 85_000m, "Fremennik shipwreck - two rune hooks"),       // 80
            Band(3_972_294, 95_000m, "Merchant shipwreck - two dragon hooks")       // 87
        ];
        if (useCrystalExtractor)
            bands = bands.Select(band => band.StartExperience >= SailingGlobal.CrystalExtractorUnlockExperience
                ? band with
                {
                    ExperiencePerHour = band.ExperiencePerHour + SailingGlobal.CrystalExtractorExperiencePerHour,
                    ConfigurationRateAddition = SailingGlobal.CrystalExtractorExperiencePerHour
                }
                : band).ToArray();

        return new("salvaging", "Salvaging", bands,
            "Active salvaging: player on one hook, crew on the second from the level-50 sloop. " +
            "Uses available hook tiers and Cabin Boy Jenkins from level 60 (Dragon Slayer I required). " +
            "Rates are estimates from published ranges, not guaranteed per-level success calculations; " +
            "the Wiki flags its rates as potentially outdated. No tick manipulation or level boosts. " +
            "Assumes the required Construction levels, schematics and navigation protection; " +
            "on-board sorting requires level 42 Sailing/34 Construction. Banking travel is excluded. " +
            "Fisherman's wrecks unlock at 26, but the route retains small wrecks until the " +
            "steel-hook estimate is available at 27. Rune hooks are retained at Fremennik wrecks because " +
            "a separate dragon-hook rate there is unverified. Merchant uses dragon hooks from level 87. " +
            "Extractor adds 250 XP every 63 seconds from level 73 when enabled; it generates wind motes, " +
            "and can also grant crystal shards after Song of the Elves; shard income is not modelled here. " +
            "Loot, boat upgrades, supplies and extractor outputs are not priced. " +
            "Below 15, the existing Gwenith planning placeholder is retained; those early hours are not a verified route.",
            UseStableDisplayName: true);
    }
}
