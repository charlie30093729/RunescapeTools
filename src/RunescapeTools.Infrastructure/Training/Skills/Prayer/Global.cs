using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer;

internal static class PrayerGlobal
{
    public const string OfferingLocationKey = "offering-location";
    private const string GildedAltar = "gilded-altar";
    private const string ChaosAltar = "chaos-altar";

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    OfferingLocationKey,
                    "Offering location",
                    TrainingConfigurationOptionKind.Choice,
                    GildedAltar,
                    "Select where the chosen bones are offered.",
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
            ConfigureMethod);

    public static PrayerSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration
                     ?? Configurator.Definition.Normalize();
        return new PrayerSettings(values.GetChoice(OfferingLocationKey));
    }

    public static bool UsesChaosAltar(PrayerSettings settings) =>
        string.Equals(settings.OfferingLocation, ChaosAltar, StringComparison.OrdinalIgnoreCase);

    public static string LocationName(PrayerSettings settings) =>
        UsesChaosAltar(settings) ? "Chaos Altar" : "Gilded Altar";

    private static TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues values)
    {
        var settings = ResolveSettings(values);
        return method.Id switch
        {
            "superior-dragon-bones" => Methods.SuperiorDragonBones.Create(settings),
            "dragon-bones" => Methods.DragonBones.Create(settings),
            "frost-dragon-bones" => Methods.FrostDragonBones.Create(settings),
            _ => method
        };
    }

    internal readonly record struct PrayerSettings(string OfferingLocation);
}
