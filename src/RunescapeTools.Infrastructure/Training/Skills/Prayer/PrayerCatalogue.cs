using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class PrayerCatalogue
{
    private const decimal EffectiveChaosAltarXpPerSuperiorBone = 1_050m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Prayer",
            Band(0, 1_670_000m, "Dagannoth bones at the chaos altar"),
            Band(
                737_627,
                2_000_000m,
                "Superior dragon bones at the Chaos Altar",
                new TrainingEconomics(
                    [Input(Items.SuperiorDragonBones, 1m / EffectiveChaosAltarXpPerSuperiorBone)])));

    private static class Items
    {
        public static readonly CatalogueItem SuperiorDragonBones = new(22124, "Superior dragon bones");
    }
}
