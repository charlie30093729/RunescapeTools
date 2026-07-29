using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore;

internal static class HerbloreGlobal
{
    private const decimal PrescriptionGogglesSaveChance = 0.10m;
    private const decimal AlchemistsAmuletExtraDoseChance = 0.15m;
    private const decimal AlchemistsAmuletChargesPerChemistryAmulet = 10m;
    private const decimal DosesPerSaleItem = 4m;

    private static readonly TrainingRateBand[] MainRouteBands =
    [
        Band(0, 11_100m, "Quests"),
        Band(8_025, 218_750m, "Serum 207s"),
        Band(123_660, 293_750m, "Super energies"),
        Band(166_636, 312_500m, "Super strengths"),
        Band(368_599, 356_250m, "Super restores"),
        Band(496_254, 375_000m, "Super defences"),
        Band(668_051, 393_750m, "Antifire potions"),
        Band(899_257, 406_250m, "Ranging potions"),
        Band(1_336_443, 431_250m, "Magic potions"),
        Band(1_475_581, 535_500m, "1t stamina potions")
    ];

    public const string EquipmentNote =
        "Eligible potion economics use Prescription goggles to save 10% of secondary ingredients " +
        "on average and a charged Alchemist's amulet to add one dose 15% of the time. Amulet of " +
        "chemistry charges are priced at 0.015 amulets per eligible potion. Finished potions are " +
        "decanted and sold as four-dose items. Methods that cannot use an equipment effect opt out " +
        "explicitly; one-time untradeable equipment costs are excluded.";

    public static IReadOnlyList<TrainingRateBand> CreateRoute(
        TrainingRateBand selectedMethodBand,
        params TrainingRateBand[] precedingMethodBands) =>
        MainRouteBands
            .Concat(precedingMethodBands)
            .Where(band => band.StartExperience < selectedMethodBand.StartExperience)
            .Append(selectedMethodBand)
            .OrderBy(band => band.StartExperience)
            .ToArray();

    public static TrainingEconomics CreatePotionEconomics(
        CatalogueItem baseItem,
        CatalogueItem secondaryItem,
        decimal secondaryQuantityPerPotion,
        CatalogueItem outputItem,
        decimal experiencePerPotion,
        decimal baseOutputDosesPerPotion = 3m,
        bool prescriptionGogglesApply = true,
        bool alchemistsAmuletApplies = true)
    {
        if (secondaryQuantityPerPotion <= 0m)
            throw new ArgumentOutOfRangeException(nameof(secondaryQuantityPerPotion));
        if (experiencePerPotion <= 0m)
            throw new ArgumentOutOfRangeException(nameof(experiencePerPotion));
        if (baseOutputDosesPerPotion <= 0m)
            throw new ArgumentOutOfRangeException(nameof(baseOutputDosesPerPotion));

        var secondaryMultiplier = prescriptionGogglesApply
            ? 1m - PrescriptionGogglesSaveChance
            : 1m;
        var resources = new List<TrainingResourceFlow>
        {
            Input(baseItem, 1m / experiencePerPotion),
            Input(
                secondaryItem,
                secondaryQuantityPerPotion * secondaryMultiplier / experiencePerPotion)
        };

        if (alchemistsAmuletApplies)
        {
            resources.Add(
                Input(
                    Items.AmuletOfChemistry,
                    AlchemistsAmuletExtraDoseChance
                    / AlchemistsAmuletChargesPerChemistryAmulet
                    / experiencePerPotion));
        }

        var expectedOutputDoses =
            baseOutputDosesPerPotion
            + (alchemistsAmuletApplies ? AlchemistsAmuletExtraDoseChance : 0m);
        resources.Add(
            Output(
                outputItem,
                expectedOutputDoses / DosesPerSaleItem / experiencePerPotion));

        return new TrainingEconomics(resources);
    }

    private static class Items
    {
        public static readonly CatalogueItem AmuletOfChemistry = new(21163, "Amulet of chemistry");
    }
}
