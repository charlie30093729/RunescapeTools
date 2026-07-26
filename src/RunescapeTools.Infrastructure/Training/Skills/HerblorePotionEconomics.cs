using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class HerblorePotionEconomics
{
    private const decimal PrescriptionGogglesSaveChance = 0.10m;
    private const decimal AlchemistsAmuletExtraDoseChance = 0.15m;
    private const decimal AlchemistsAmuletChargesPerChemistryAmulet = 10m;

    public const string EquipmentNote =
        "Potion economics use the shared Herblore equipment model: Prescription goggles save " +
        "10% of eligible secondary ingredients on average, and a charged Alchemist's amulet has " +
        "a 15% chance to add one dose. Amulet of chemistry charges are priced at 0.015 amulets " +
        "per eligible potion. Future potion routes include both effects by default unless they " +
        "explicitly opt out. The one-time cost of obtaining the untradeable equipment is excluded.";

    public static TrainingEconomics Create(
        int baseItemId,
        string baseItemName,
        int secondaryItemId,
        string secondaryItemName,
        int outputItemId,
        string outputItemName,
        decimal experiencePerPotion,
        decimal dosesPerOutputItem = 3m,
        bool prescriptionGogglesApply = true,
        bool alchemistsAmuletApplies = true)
    {
        if (experiencePerPotion <= 0m)
            throw new ArgumentOutOfRangeException(nameof(experiencePerPotion));
        if (dosesPerOutputItem <= 0m)
            throw new ArgumentOutOfRangeException(nameof(dosesPerOutputItem));

        var resources = new List<TrainingResourceFlow>
        {
            Input(baseItemId, baseItemName, 1m / experiencePerPotion),
            Input(
                secondaryItemId,
                secondaryItemName,
                (prescriptionGogglesApply ? 1m - PrescriptionGogglesSaveChance : 1m)
                / experiencePerPotion)
        };

        if (alchemistsAmuletApplies)
        {
            resources.Add(
                Input(
                    AmuletOfChemistry,
                    "Amulet of chemistry",
                    AlchemistsAmuletExtraDoseChance
                    / AlchemistsAmuletChargesPerChemistryAmulet
                    / experiencePerPotion));
        }

        var outputDosesPerPotion =
            dosesPerOutputItem + (alchemistsAmuletApplies ? AlchemistsAmuletExtraDoseChance : 0m);
        resources.Add(
            Output(
                outputItemId,
                outputItemName,
                outputDosesPerPotion / dosesPerOutputItem / experiencePerPotion));

        return new TrainingEconomics(resources);
    }
}
