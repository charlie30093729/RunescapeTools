using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Thieving.Methods;

internal static class ArdougneKnights
{
    private const string Name = "Ardougne knights";
    private const long UnlockExperience = 166_636; // Level 55.
    private const decimal ExperiencePerPickpocket = 84.3m;
    private const decimal CoinsPerPickpocket = 50m;
    private const decimal FullRogueLootMultiplier = 2m;

    public static TrainingMethodDefinition Create() => new(
        "ardougne-knights", Name,
        [
            .. GemKnights.Create().Bands.Where(band => band.StartExperience < UnlockExperience),
            CreateBand(UnlockExperience, 82_369m),
            CreateBand(273_742, 93_894m),
            CreateBand(449_428, 105_889m),
            CreateBand(737_627, 121_154m),
            CreateBand(1_210_421, 139_152m),
            CreateBand(1_986_068, 158_557m),
            CreateBand(3_258_594, 184_303m),
            CreateBand(5_346_332, 216_281m),
            CreateBand(8_771_558, 252_900m)
        ],
        "Requires level 55 Thieving. Assumes the medium Ardougne Diary's 10% pickpocket-success " +
        "bonus and full Rogue equipment throughout. Efficient rates use the Wiki's diary-only " +
        "chart at levels 55/60/65/70/75/80/85/90/95, held until the next band; no dodgy necklaces " +
        "or Shadow Veil are assumed. At 95+ the 252,900 XP/hour preset is the tick-perfect " +
        "3,000-successful-pickpocket ceiling. Missed clicks, movement and coin-pouch handling " +
        "can reduce actual rates; enter a personal rate as needed. Each success grants 84.3 XP " +
        "and 100 coins with full Rogue equipment, giving approximately +1.18624 GP/XP, without " +
        "market prices or GE tax. Food/healing below 95, optional supplies, travel, equipment " +
        "acquisition and pet value are excluded: lower-level GP is gross coin yield. The " +
        "existing Gem knights projection is retained before 55 for compatibility, not presented " +
        "as a verified lower-level training route.",
        UseStableDisplayName: true);

    private static TrainingRateBand CreateBand(long startExperience, decimal rate) =>
        Band(startExperience, rate, Name, new TrainingEconomics([],
            FixedGpOutputPerExperience: CoinsPerPickpocket * FullRogueLootMultiplier
                                       / ExperiencePerPickpocket));
}
