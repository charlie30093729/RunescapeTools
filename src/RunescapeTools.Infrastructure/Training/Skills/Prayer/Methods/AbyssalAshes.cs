using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer.Methods;

internal static class AbyssalAshes
{
    private static readonly CatalogueItem Ashes = new(25775, "Abyssal ashes");

    public static TrainingMethodDefinition Create(PrayerGlobal.PrayerSettings settings) =>
        PrayerGlobal.CreateOfferingMethod("abyssal-ashes", Ashes.Name, Ashes, 85m,
            settings, ashes: true);
}
