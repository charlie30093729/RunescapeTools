using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class MagicCatalogue
{
    private const decimal ExperiencePerHour = 330_000m;
    private const decimal CastsPerHour = 1_085m;
    private const decimal KodaiRuneConsumption = 0.85m;

    public static TrainingSkillDefinition Create() =>
        new(
            "Magic",
            [
                Band(
                    0,
                    ExperiencePerHour,
                    "Ice Barrage",
                    new TrainingEconomics(
                        [
                            Input(
                                BloodRune,
                                "Blood rune",
                                2m * KodaiRuneConsumption * CastsPerHour / ExperiencePerHour),
                            Input(
                                DeathRune,
                                "Death rune",
                                4m * KodaiRuneConsumption * CastsPerHour / ExperiencePerHour)
                        ]))
            ],
            IsZeroTime: true,
            Note: "Ice Barrage is priced only against Magic XP left after pending Slayer credit and contributes " +
                  "zero active hours. The reviewed 330,000 XP/hour cost model assumes 1,085 casts/hour, " +
                  "a Kodai wand's 15% rune-saving effect, and no water-rune cost.");
}
