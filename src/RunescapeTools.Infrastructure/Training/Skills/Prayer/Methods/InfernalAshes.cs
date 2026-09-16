using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer.Methods;

internal static class InfernalAshes
{
    private static readonly CatalogueItem Ashes = new(25778, "Infernal ashes");

    public static TrainingMethodDefinition Create(PrayerGlobal.PrayerSettings settings) =>
        PrayerGlobal.CreateOfferingMethod("infernal-ashes", Ashes.Name, Ashes, 110m,
            settings, ashes: true);
}
