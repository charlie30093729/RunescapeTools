using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.Skills.Farming.FarmingGlobal;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Farming.Methods;

internal static class MagicDragonfruitTreeRuns
{
    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "Magic + dragonfruit tree runs",
            [
                .. CreateBaseBands(),
                Band(
                    2_192_818,
                    2_475_000m,
                    "Efficient tree runs - magic + dragonfruit",
                    TreeRunEconomics(Level81Components())),
                Band(
                    3_258_594,
                    2_611_000m,
                    "Efficient tree runs - magic + dragonfruit",
                    TreeRunEconomics(Level85Components())),
                Band(
                    6_517_253,
                    2_669_000m,
                    "Efficient tree runs - magic + dragonfruit",
                    TreeRunEconomics(Level92Components()))
            ],
            Note);

    private static TreeComponent[] Level81Components() =>
    [
        Regular(Items.MagicSapling, Items.Coconut, 25m, 13_913.8m),
        Fruit(MethodItems.DragonfruitSapling, Items.Coconut, 15m, 17_475m),
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
        Fruit(MethodItems.DragonfruitSapling, Items.Coconut, 15m, 17_475m),
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

    private static class MethodItems
    {
        public static readonly CatalogueItem DragonfruitSapling = new(22866, "Dragonfruit sapling");
    }
}
