using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class FarmingCatalogue
{
    private const decimal RegularTreesPerDay = 6m;
    private const decimal FruitTreesPerDay = 6m;
    private const decimal HardwoodPatches = 4m;
    private const decimal StandardClearingFee = 200m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Farming",
            "Efficient tree-run rates represent active player time. Economics buy saplings and every protection " +
            "payment at live high prices, include gardener clearing fees, and assume one six-tree and six-fruit-tree " +
            "run per day. Four hardwood patches and the redwood patch are normalized by their growth cycles; calquat " +
            "and celastrus are completed daily. Fruit, bark, and logs are not harvested or valued.",
            Band(0, 16_000m, "Quests"),
            Band(
                32_500,
                364_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(WillowSapling, "Willow sapling", CookingApple, "Cooking apple", 5m, 1_481.5m),
                    Fruit(BananaSapling, "Banana sapling", CookingApple, "Cooking apple", 20m, 1_778.5m),
                    Hardwood(TeakSapling, "Teak sapling", LimpwurtRoot, "Limpwurt root", 15m, 7_325m, 74m + 40m / 60m))),
            Band(
                61_512,
                575_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(MapleSapling, "Maple sapling", Orange, "Orange", 5m, 3_448.4m),
                    Fruit(CurrySapling, "Curry sapling", Banana, "Banana", 25m, 2_946.9m),
                    Hardwood(TeakSapling, "Teak sapling", LimpwurtRoot, "Limpwurt root", 15m, 7_325m, 74m + 40m / 60m))),
            Band(
                166_636,
                841_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(MapleSapling, "Maple sapling", Orange, "Orange", 5m, 3_448.4m),
                    Fruit(PineappleSapling, "Pineapple sapling", Watermelon, "Watermelon", 10m, 4_662.7m),
                    Hardwood(MahoganySapling, "Mahogany sapling", YanillianHops, "Yanillian hops", 25m, 15_783m, 85m + 20m / 60m))),
            Band(
                273_742,
                1_222_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(YewSapling, "Yew sapling", CactusSpine, "Cactus spine", 10m, 7_150.9m),
                    Fruit(PapayaSapling, "Papaya sapling", Pineapple, "Pineapple", 10m, 6_218.4m),
                    Hardwood(MahoganySapling, "Mahogany sapling", YanillianHops, "Yanillian hops", 25m, 15_783m, 85m + 20m / 60m))),
            Band(
                605_032,
                1_428_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(YewSapling, "Yew sapling", CactusSpine, "Cactus spine", 10m, 7_150.9m),
                    Fruit(PalmSapling, "Palm sapling", PapayaFruit, "Papaya fruit", 15m, 10_260.6m),
                    Hardwood(CamphorSapling, "Camphor sapling", WhiteBerries, "White berries", 10m, 17_928m, 85m + 20m / 60m))),
            Band(
                1_210_421,
                2_063_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(MagicSapling, "Magic sapling", Coconut, "Coconut", 25m, 13_913.8m),
                    Fruit(PalmSapling, "Palm sapling", PapayaFruit, "Papaya fruit", 15m, 10_260.6m),
                    Hardwood(CamphorSapling, "Camphor sapling", WhiteBerries, "White berries", 10m, 17_928m, 85m + 20m / 60m),
                    Daily(CalquatSapling, "Calquat sapling", PoisonIvyBerries, "Poison ivy berries", 8m, 12_225.5m))),
            Band(
                2_192_818,
                2_475_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(MagicSapling, "Magic sapling", Coconut, "Coconut", 25m, 13_913.8m),
                    Fruit(DragonfruitSapling, "Dragonfruit sapling", Coconut, "Coconut", 15m, 17_475m),
                    Hardwood(IronwoodSapling, "Ironwood sapling", CurryLeaf, "Curry leaf", 10m, 20_525m, 85m + 20m / 60m),
                    Daily(CalquatSapling, "Calquat sapling", PoisonIvyBerries, "Poison ivy berries", 8m, 12_225.5m))),
            Band(
                3_258_594,
                2_611_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(MagicSapling, "Magic sapling", Coconut, "Coconut", 25m, 13_913.8m),
                    Fruit(DragonfruitSapling, "Dragonfruit sapling", Coconut, "Coconut", 15m, 17_475m),
                    Hardwood(IronwoodSapling, "Ironwood sapling", CurryLeaf, "Curry leaf", 10m, 20_525m, 85m + 20m / 60m),
                    Daily(CalquatSapling, "Calquat sapling", PoisonIvyBerries, "Poison ivy berries", 8m, 12_225.5m),
                    Daily(CelastrusSapling, "Celastrus sapling", PotatoCactus, "Potato cactus", 8m, 14_334m))),
            Band(
                6_517_253,
                2_669_000m,
                "Efficient tree runs",
                TreeRunEconomics(
                    Regular(MagicSapling, "Magic sapling", Coconut, "Coconut", 25m, 13_913.8m),
                    Fruit(DragonfruitSapling, "Dragonfruit sapling", Coconut, "Coconut", 15m, 17_475m),
                    Hardwood(RosewoodSapling, "Rosewood sapling", Dragonfruit, "Dragonfruit", 8m, 23_352m, 96m),
                    Daily(CalquatSapling, "Calquat sapling", PoisonIvyBerries, "Poison ivy berries", 8m, 12_225.5m),
                    Daily(CelastrusSapling, "Celastrus sapling", PotatoCactus, "Potato cactus", 8m, 14_334m),
                    EveryHours(
                        RedwoodSapling,
                        "Redwood sapling",
                        Dragonfruit,
                        "Dragonfruit",
                        6m,
                        22_680m,
                        106m + 40m / 60m,
                        clearingFee: 2_000m))));

    private static TreeComponent Regular(
        int saplingId,
        string saplingName,
        int paymentId,
        string paymentName,
        decimal paymentQuantity,
        decimal experiencePerTree) =>
        new(
            saplingId,
            saplingName,
            paymentId,
            paymentName,
            paymentQuantity,
            experiencePerTree,
            RegularTreesPerDay,
            StandardClearingFee);

    private static TreeComponent Fruit(
        int saplingId,
        string saplingName,
        int paymentId,
        string paymentName,
        decimal paymentQuantity,
        decimal experiencePerTree) =>
        new(
            saplingId,
            saplingName,
            paymentId,
            paymentName,
            paymentQuantity,
            experiencePerTree,
            FruitTreesPerDay,
            StandardClearingFee);

    private static TreeComponent Hardwood(
        int saplingId,
        string saplingName,
        int paymentId,
        string paymentName,
        decimal paymentQuantity,
        decimal experiencePerTree,
        decimal growthHours) =>
        new(
            saplingId,
            saplingName,
            paymentId,
            paymentName,
            paymentQuantity,
            experiencePerTree,
            HardwoodPatches * 24m / growthHours,
            StandardClearingFee);

    private static TreeComponent Daily(
        int saplingId,
        string saplingName,
        int paymentId,
        string paymentName,
        decimal paymentQuantity,
        decimal experiencePerTree) =>
        new(
            saplingId,
            saplingName,
            paymentId,
            paymentName,
            paymentQuantity,
            experiencePerTree,
            1m,
            StandardClearingFee);

    private static TreeComponent EveryHours(
        int saplingId,
        string saplingName,
        int paymentId,
        string paymentName,
        decimal paymentQuantity,
        decimal experiencePerTree,
        decimal growthHours,
        decimal clearingFee) =>
        new(
            saplingId,
            saplingName,
            paymentId,
            paymentName,
            paymentQuantity,
            experiencePerTree,
            24m / growthHours,
            clearingFee);

    private static TrainingEconomics TreeRunEconomics(params TreeComponent[] components)
    {
        var experiencePerDay = components.Sum(component =>
            component.ExperiencePerTree * component.TreesPerDay);
        var resources = components
            .SelectMany(component =>
                new[]
                {
                    Input(
                        component.SaplingId,
                        component.SaplingName,
                        component.TreesPerDay / experiencePerDay),
                    Input(
                        component.PaymentId,
                        component.PaymentName,
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

    private sealed record TreeComponent(
        int SaplingId,
        string SaplingName,
        int PaymentId,
        string PaymentName,
        decimal PaymentQuantity,
        decimal ExperiencePerTree,
        decimal TreesPerDay,
        decimal ClearingFee);
}
