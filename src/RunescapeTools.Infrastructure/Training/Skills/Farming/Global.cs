using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Farming;

internal static class FarmingGlobal
{
    private const decimal RegularTreesPerDay = 6m;
    private const decimal FruitTreesPerDay = 6m;
    private const decimal HardwoodPatches = 4m;
    private const decimal StandardClearingFee = 200m;

    public const string Note =
        "Efficient tree-run rates represent active player time. Economics buy saplings and every protection " +
        "payment at live high prices, include gardener clearing fees, and assume one six-tree and six-fruit-tree " +
        "run per day. Four hardwood patches and the redwood patch are normalized by their growth cycles; calquat " +
        "and celastrus are completed daily. Fruit, bark, and logs are not harvested or valued.";

    public static IReadOnlyList<TrainingRateBand> CreateBaseBands() =>
    [
        Band(0, 16_000m, "Quests"),
        Band(
            32_500,
            364_000m,
            "Efficient tree runs",
            TreeRunEconomics(
                Regular(Items.WillowSapling, Items.CookingApple, 5m, 1_481.5m),
                Fruit(Items.BananaSapling, Items.CookingApple, 20m, 1_778.5m),
                Hardwood(Items.TeakSapling, Items.LimpwurtRoot, 15m, 7_325m, 74m + 40m / 60m))),
        Band(
            61_512,
            575_000m,
            "Efficient tree runs",
            TreeRunEconomics(
                Regular(Items.MapleSapling, Items.Orange, 5m, 3_448.4m),
                Fruit(Items.CurrySapling, Items.Banana, 25m, 2_946.9m),
                Hardwood(Items.TeakSapling, Items.LimpwurtRoot, 15m, 7_325m, 74m + 40m / 60m))),
        Band(
            166_636,
            841_000m,
            "Efficient tree runs",
            TreeRunEconomics(
                Regular(Items.MapleSapling, Items.Orange, 5m, 3_448.4m),
                Fruit(Items.PineappleSapling, Items.Watermelon, 10m, 4_662.7m),
                Hardwood(Items.MahoganySapling, Items.YanillianHops, 25m, 15_783m, 85m + 20m / 60m))),
        Band(
            273_742,
            1_222_000m,
            "Efficient tree runs",
            TreeRunEconomics(
                Regular(Items.YewSapling, Items.CactusSpine, 10m, 7_150.9m),
                Fruit(Items.PapayaSapling, Items.Pineapple, 10m, 6_218.4m),
                Hardwood(Items.MahoganySapling, Items.YanillianHops, 25m, 15_783m, 85m + 20m / 60m))),
        Band(
            605_032,
            1_428_000m,
            "Efficient tree runs",
            TreeRunEconomics(
                Regular(Items.YewSapling, Items.CactusSpine, 10m, 7_150.9m),
                Fruit(Items.PalmSapling, Items.PapayaFruit, 15m, 10_260.6m),
                Hardwood(Items.CamphorSapling, Items.WhiteBerries, 10m, 17_928m, 85m + 20m / 60m))),
        Band(
            1_210_421,
            2_063_000m,
            "Efficient tree runs",
            TreeRunEconomics(
                Regular(Items.MagicSapling, Items.Coconut, 25m, 13_913.8m),
                Fruit(Items.PalmSapling, Items.PapayaFruit, 15m, 10_260.6m),
                Hardwood(Items.CamphorSapling, Items.WhiteBerries, 10m, 17_928m, 85m + 20m / 60m),
                Daily(Items.CalquatSapling, Items.PoisonIvyBerries, 8m, 12_225.5m)))
    ];

    public static TreeComponent Regular(
        CatalogueItem sapling,
        CatalogueItem payment,
        decimal paymentQuantity,
        decimal experiencePerTree) =>
        new(
            sapling,
            payment,
            paymentQuantity,
            experiencePerTree,
            RegularTreesPerDay,
            StandardClearingFee);

    public static TreeComponent Fruit(
        CatalogueItem sapling,
        CatalogueItem payment,
        decimal paymentQuantity,
        decimal experiencePerTree) =>
        new(
            sapling,
            payment,
            paymentQuantity,
            experiencePerTree,
            FruitTreesPerDay,
            StandardClearingFee);

    public static TreeComponent Hardwood(
        CatalogueItem sapling,
        CatalogueItem payment,
        decimal paymentQuantity,
        decimal experiencePerTree,
        decimal growthHours) =>
        new(
            sapling,
            payment,
            paymentQuantity,
            experiencePerTree,
            HardwoodPatches * 24m / growthHours,
            StandardClearingFee);

    public static TreeComponent Daily(
        CatalogueItem sapling,
        CatalogueItem payment,
        decimal paymentQuantity,
        decimal experiencePerTree) =>
        new(
            sapling,
            payment,
            paymentQuantity,
            experiencePerTree,
            1m,
            StandardClearingFee);

    public static TreeComponent EveryHours(
        CatalogueItem sapling,
        CatalogueItem payment,
        decimal paymentQuantity,
        decimal experiencePerTree,
        decimal growthHours,
        decimal clearingFee) =>
        new(
            sapling,
            payment,
            paymentQuantity,
            experiencePerTree,
            24m / growthHours,
            clearingFee);

    public static decimal ScaleRateForFruitReplacement(
        decimal originalRate,
        IReadOnlyList<TreeComponent> replacementComponents,
        decimal originalFruitExperience,
        decimal replacementFruitExperience)
    {
        var replacementExperiencePerDay = ExperiencePerDay(replacementComponents);
        var originalExperiencePerDay =
            replacementExperiencePerDay
            + FruitTreesPerDay * (originalFruitExperience - replacementFruitExperience);
        return originalRate * replacementExperiencePerDay / originalExperiencePerDay;
    }

    public static TrainingEconomics TreeRunEconomics(params TreeComponent[] components)
    {
        var experiencePerDay = ExperiencePerDay(components);
        var resources = components
            .SelectMany(component =>
                new[]
                {
                    Input(
                        component.Sapling,
                        component.TreesPerDay / experiencePerDay),
                    Input(
                        component.Payment,
                        component.PaymentQuantity * component.TreesPerDay / experiencePerDay)
                })
            .GroupBy(resource => (resource.ItemId, resource.Name, resource.Direction))
            .Select(group => group.Aggregate((left, right) =>
                left with
                {
                    QuantityPerExperience =
                        left.QuantityPerExperience + right.QuantityPerExperience
                }))
            .ToArray();
        var clearingGpPerExperience = components.Sum(component =>
            component.ClearingFee * component.TreesPerDay) / experiencePerDay;

        return new TrainingEconomics(
            resources,
            FixedGpPerExperience: clearingGpPerExperience);
    }

    private static decimal ExperiencePerDay(IEnumerable<TreeComponent> components) =>
        components.Sum(component => component.ExperiencePerTree * component.TreesPerDay);

    internal sealed record TreeComponent(
        CatalogueItem Sapling,
        CatalogueItem Payment,
        decimal PaymentQuantity,
        decimal ExperiencePerTree,
        decimal TreesPerDay,
        decimal ClearingFee);

    internal static class Items
    {
        public static readonly CatalogueItem Banana = new(1963, "Banana");
        public static readonly CatalogueItem BananaSapling = new(5497, "Banana sapling");
        public static readonly CatalogueItem CactusSpine = new(6016, "Cactus spine");
        public static readonly CatalogueItem CalquatSapling = new(5503, "Calquat sapling");
        public static readonly CatalogueItem CamphorSapling = new(31502, "Camphor sapling");
        public static readonly CatalogueItem CelastrusSapling = new(22856, "Celastrus sapling");
        public static readonly CatalogueItem Coconut = new(5974, "Coconut");
        public static readonly CatalogueItem CookingApple = new(1955, "Cooking apple");
        public static readonly CatalogueItem CurryLeaf = new(5970, "Curry leaf");
        public static readonly CatalogueItem CurrySapling = new(5499, "Curry sapling");
        public static readonly CatalogueItem Dragonfruit = new(22929, "Dragonfruit");
        public static readonly CatalogueItem IronwoodSapling = new(31505, "Ironwood sapling");
        public static readonly CatalogueItem LimpwurtRoot = new(225, "Limpwurt root");
        public static readonly CatalogueItem MagicSapling = new(5374, "Magic sapling");
        public static readonly CatalogueItem MahoganySapling = new(21480, "Mahogany sapling");
        public static readonly CatalogueItem MapleSapling = new(5372, "Maple sapling");
        public static readonly CatalogueItem Orange = new(2108, "Orange");
        public static readonly CatalogueItem PalmSapling = new(5502, "Palm sapling");
        public static readonly CatalogueItem PapayaFruit = new(5972, "Papaya fruit");
        public static readonly CatalogueItem PapayaSapling = new(5501, "Papaya sapling");
        public static readonly CatalogueItem Pineapple = new(2114, "Pineapple");
        public static readonly CatalogueItem PineappleSapling = new(5500, "Pineapple sapling");
        public static readonly CatalogueItem PoisonIvyBerries = new(6018, "Poison ivy berries");
        public static readonly CatalogueItem PotatoCactus = new(3138, "Potato cactus");
        public static readonly CatalogueItem RedwoodSapling = new(22859, "Redwood sapling");
        public static readonly CatalogueItem RosewoodSapling = new(31508, "Rosewood sapling");
        public static readonly CatalogueItem TeakSapling = new(21477, "Teak sapling");
        public static readonly CatalogueItem Watermelon = new(5982, "Watermelon");
        public static readonly CatalogueItem WhiteBerries = new(239, "White berries");
        public static readonly CatalogueItem WillowSapling = new(5371, "Willow sapling");
        public static readonly CatalogueItem YanillianHops = new(5998, "Yanillian hops");
        public static readonly CatalogueItem YewSapling = new(5373, "Yew sapling");
    }
}
