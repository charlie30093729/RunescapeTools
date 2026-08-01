using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Crafting;

internal static class CraftingGlobal
{
    private static readonly TrainingRateBand[] MainRouteBands =
    [
        Band(0, 37_000m, "Leather items"),
        Band(4_470, 139_000m, "Sapphires"),
        Band(9_730, 187_650m, "Emeralds"),
        Band(20_224, 236_300m, "Rubies"),
        Band(50_339, 298_850m, "Diamonds"),
        Band(368_599, 335_230m, "Green d'hide bodies"),
        Band(814_445, 378_490m, "Blue d'hide bodies"),
        Band(1_475_581, 421_740m, "Red d'hide bodies")
    ];

    public static IReadOnlyList<TrainingRateBand> CreateRoute(TrainingRateBand selectedMethodBand) =>
        MainRouteBands
            .Where(band => band.StartExperience < selectedMethodBand.StartExperience)
            .Append(selectedMethodBand)
            .OrderBy(band => band.StartExperience)
            .ToArray();
}
