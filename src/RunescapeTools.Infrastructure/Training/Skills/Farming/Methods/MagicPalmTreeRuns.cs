using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.Skills.Farming.FarmingGlobal;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Farming.Methods;

internal static class MagicPalmTreeRuns
{
    private const decimal DragonfruitExperiencePerTree = 17_475m;
    private const decimal PalmExperiencePerTree = 10_260.6m;

    public static TrainingMethodDefinition Create()
    {
        var level81 = Level81Components();
        var level85 = Level85Components();
        var level92 = Level92Components();

        return new TrainingMethodDefinition(
            "magic-palm-tree-runs",
            "Magic + palm tree runs",
            [
                .. CreateBaseBands(),
                Band(
                    2_192_818,
                    PalmRate(2_475_000m, level81),
                    "Efficient tree runs - magic + palm",
                    TreeRunEconomics(level81)),
                Band(
                    3_258_594,
                    PalmRate(2_611_000m, level85),
                    "Efficient tree runs - magic + palm",
                    TreeRunEconomics(level85)),
                Band(
                    6_517_253,
                    PalmRate(2_669_000m, level92),
                    "Efficient tree runs - magic + palm",
                    TreeRunEconomics(level92))
            ],
            Note + " The palm alternative retains palm trees after dragonfruit trees unlock; its active rates " +
            "are scaled by the resulting daily XP while preserving the reviewed run-time assumptions.");
    }

    private static decimal PalmRate(decimal originalRate, IReadOnlyList<TreeComponent> components) =>
        ScaleRateForFruitReplacement(
            originalRate,
            components,
            DragonfruitExperiencePerTree,
            PalmExperiencePerTree);

    private static TreeComponent[] Level81Components() =>
    [
        Regular(Items.MagicSapling, Items.Coconut, 25m, 13_913.8m),
        Fruit(Items.PalmSapling, Items.PapayaFruit, 15m, PalmExperiencePerTree),
        Hardwood(Items.IronwoodSapling, Items.CurryLeaf, 10m, 20_525m, 85m + 20m / 60m),
        Daily(Items.CalquatSapling, Items.PoisonIvyBerries, 8m, 12_225.5m)
    ];

    private static TreeComponent[] Level85Components() =>
    [
        .. Level81Components(),
        Daily(Items.CelastrusSapling, Items.PotatoCactus, 8m, 14_334m)
    ];

    private static TreeComponent[] Level92Components() =>
    [
        Regular(Items.MagicSapling, Items.Coconut, 25m, 13_913.8m),
        Fruit(Items.PalmSapling, Items.PapayaFruit, 15m, PalmExperiencePerTree),
        Hardwood(Items.RosewoodSapling, Items.Dragonfruit, 8m, 23_352m, 96m),
        Daily(Items.CalquatSapling, Items.PoisonIvyBerries, 8m, 12_225.5m),
        Daily(Items.CelastrusSapling, Items.PotatoCactus, 8m, 14_334m),
        EveryHours(
            Items.RedwoodSapling,
            Items.Dragonfruit,
            6m,
            22_680m,
            106m + 40m / 60m,
            clearingFee: 2_000m)
    ];
}
