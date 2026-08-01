using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer.Methods;

internal static class DragonBones
{
    private const decimal GildedExperiencePerHour = 642_600m;
    private const decimal ChaosExperiencePerHour = 504_000m;
    private const decimal GildedExperiencePerBone = 252m;
    private const decimal EffectiveChaosExperiencePerBone = 504m;

    public static TrainingMethodDefinition Create(PrayerGlobal.PrayerSettings settings)
    {
        var chaos = PrayerGlobal.UsesChaosAltar(settings);
        return new TrainingMethodDefinition(
            "dragon-bones",
            "Dragon bones",
            [
                Band(
                    0,
                    chaos ? ChaosExperiencePerHour : GildedExperiencePerHour,
                    $"Dragon bones at the {PrayerGlobal.LocationName(settings)}",
                    new TrainingEconomics(
                    [
                        Input(
                            Items.DragonBones,
                            1m / (chaos ? EffectiveChaosExperiencePerBone : GildedExperiencePerBone))
                    ]))
            ],
            "Rates assume manual offering: 2,550 bones/hour at a Gilded Altar or 2,000 offerings/hour at the Chaos Altar. Chaos Altar economics include its 50% bone-save chance, but exclude deaths, supplies, and unnoting fees.",
            UseStableDisplayName: true);
    }

    private static class Items
    {
        public static readonly CatalogueItem DragonBones = new(536, "Dragon bones");
    }
}
