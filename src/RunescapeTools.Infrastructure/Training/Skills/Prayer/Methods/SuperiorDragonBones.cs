using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer.Methods;

internal static class SuperiorDragonBones
{
    private const decimal ExperiencePerHour = 2_000_000m;

    public static TrainingMethodDefinition Create(PrayerGlobal.PrayerSettings settings)
    {
        var experiencePerBone = PrayerGlobal.UsesChaosAltar(settings) ? 1_050m : 525m;
        return new TrainingMethodDefinition(
            "superior-dragon-bones",
            "Superior dragon bones",
            [
                Band(
                    0,
                    ExperiencePerHour,
                    $"Superior dragon bones at the {PrayerGlobal.LocationName(settings)}",
                    new TrainingEconomics(
                    [
                        Input(Items.SuperiorDragonBones, 1m / experiencePerBone)
                    ]))
            ],
            "Retains the reviewed 2,000,000 XP/hour route; offering location changes expected bone consumption.",
            UseStableDisplayName: true);
    }

    private static class Items
    {
        public static readonly CatalogueItem SuperiorDragonBones = new(22124, "Superior dragon bones");
    }
}
