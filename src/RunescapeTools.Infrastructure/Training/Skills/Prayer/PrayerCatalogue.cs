using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

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
                    [Input(SuperiorDragonBones, "Superior dragon bones", 1m / EffectiveChaosAltarXpPerSuperiorBone)])));
}
