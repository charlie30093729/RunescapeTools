using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer;

internal static class PrayerGlobal
{
    public const string OfferingLocationKey = "offering-location";
    private const string GildedAltar = "gilded-altar";
    private const string ChaosAltar = "chaos-altar";
    private const decimal ExperiencePerHour = 2_000_000m;
    private const decimal GildedAltarExperiencePerBone = 525m;
    private const decimal EffectiveChaosAltarExperiencePerBone = 1_050m;

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    OfferingLocationKey,
                    "Offering location",
                    TrainingConfigurationOptionKind.Choice,
                    GildedAltar,
                    "Select where Superior dragon bones are offered.",
                    [
                        new TrainingConfigurationChoice(
                            GildedAltar,
                            "Gilded altar"),
                        new TrainingConfigurationChoice(
                            ChaosAltar,
                            "Chaos altar"),
                        new TrainingConfigurationChoice(
                            "offering-at-bank",
                            "Offering at a bank",
                            false,
                            "Coming soon"),
                        new TrainingConfigurationChoice(
                            "offering-at-prif-agility",
                            "Offering at Prif agility",
                            false,
                            "Coming soon")
                    ])
            ]),
            (method, values) => CreateMethod(ResolveSettings(values)));

    public static PrayerSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration
                     ?? Configurator.Definition.Normalize();
        return new PrayerSettings(values.GetChoice(OfferingLocationKey));
    }

    public static TrainingMethodDefinition CreateMethod(PrayerSettings settings)
    {
        var (location, experiencePerBone) = settings.OfferingLocation switch
        {
            ChaosAltar => ("Chaos Altar", EffectiveChaosAltarExperiencePerBone),
            _ => ("Gilded Altar", GildedAltarExperiencePerBone)
        };

        return new TrainingMethodDefinition(
            "superior-dragon-bones",
            "Superior dragon bones",
            [
                Band(
                    0,
                    ExperiencePerHour,
                    $"Superior dragon bones at the {location}",
                    new TrainingEconomics(
                    [
                        Input(Items.SuperiorDragonBones, 1m / experiencePerBone)
                    ]))
            ],
            "Only Superior dragon bones are available in this release. The rate remains the reviewed " +
            "2,000,000 XP/hour; offering location changes the expected bone consumption.",
            UseStableDisplayName: true);
    }

    internal readonly record struct PrayerSettings(string OfferingLocation);

    private static class Items
    {
        public static readonly CatalogueItem SuperiorDragonBones =
            new(22124, "Superior dragon bones");
    }
}
