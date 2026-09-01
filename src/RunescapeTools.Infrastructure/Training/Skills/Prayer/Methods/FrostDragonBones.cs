using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer.Methods;

internal static class FrostDragonBones
{
    private const decimal GildedExperiencePerHour = 892_500m;
    private const decimal ChaosExperiencePerHour = 700_000m;
    private const decimal GildedExperiencePerBone = 350m;
    private const decimal EffectiveChaosExperiencePerBone = 700m;

    public static TrainingMethodDefinition Create(PrayerGlobal.PrayerSettings settings)
    {
        var chaos = PrayerGlobal.UsesChaosAltar(settings);
        return new TrainingMethodDefinition(
            "frost-dragon-bones",
            "Frost dragon bones",
            [
                Band(
                    0,
                    chaos ? ChaosExperiencePerHour : GildedExperiencePerHour,
                    $"Frost dragon bones at the {PrayerGlobal.LocationName(settings)}",
                    new TrainingEconomics(
                    [
                        Input(
                            Items.FrostDragonBones,
                            1m / (chaos ? EffectiveChaosExperiencePerBone : GildedExperiencePerBone))
                    ]))
            ],
            "Rates assume manual offering: 2,550 bones/hour at a two-burner Gilded Altar or " +
            "2,000 offerings/hour at the Chaos Altar. Each bone gives 350 Prayer XP per " +
            "offering; Chaos Altar economics model its 50% save chance as 700 effective XP per " +
            "consumed bone, while excluding deaths, supplies, and unnoting fees.",
            UseStableDisplayName: true);
    }

    private static class Items
    {
        public static readonly CatalogueItem FrostDragonBones = new(31729, "Frost dragon bones");
    }
}
