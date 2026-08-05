using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter.Methods;

internal static class Herbiboar
{
    private const long UnlockExperience = 1_986_068;
    private const decimal CatchesPerHour = 66m;
    private const decimal TrailExperiencePerCatch = 2.56m * 50m;
    private const decimal StaminaPotionsPerCatch = 0.125m;
    private const decimal NumulitePerCatch = 14.5m * (1m / 2.778m) * 2.56m;

    private static readonly (long StartExperience, decimal CaptureExperience)[] LevelBands =
    [
        (1_986_068, 1_950m),
        (2_192_818, 1_980m),
        (2_421_087, 2_010m),
        (2_673_114, 2_040m),
        (2_951_373, 2_070m),
        (3_258_594, 2_100m),
        (3_597_792, 2_130m),
        (3_972_294, 2_160m),
        (4_385_776, 2_190m),
        (4_842_295, 2_220m),
        (5_346_332, 2_250m),
        (5_902_831, 2_280m),
        (6_517_253, 2_310m),
        (7_195_629, 2_340m),
        (7_944_614, 2_370m),
        (8_771_558, 2_385m),
        (9_684_577, 2_404m),
        (10_692_629, 2_423m),
        (11_805_606, 2_442m),
        (13_034_431, 2_461m)
    ];

    public static TrainingMethodDefinition Create()
    {
        var precedingBands = MainEhp.Create().Bands
            .Where(band => band.StartExperience < UnlockExperience);
        var herbiboarBands = LevelBands.Select(level =>
        {
            var experiencePerCatch = level.CaptureExperience + TrailExperiencePerCatch;
            return Band(
                level.StartExperience,
                experiencePerCatch * CatchesPerHour,
                "Herbiboar",
                CreateEconomics(experiencePerCatch));
        });

        return new TrainingMethodDefinition(
            "herbiboar",
            "Herbiboar",
            precedingBands.Concat(herbiboarBands).ToArray(),
            "Requires level 80 Hunter, level 31 Herblore, and Bone Voyage. Rates assume 66 catches/hour with stamina potions. Live economics use the current Wiki money-guide outputs for 99 Herblore with magic secateurs; lower Herblore levels yield fewer high-tier herbs. Fossils, Herbi, reusable equipment, and the horn of plenty are excluded.");
    }

    private static TrainingEconomics CreateEconomics(decimal experiencePerCatch) =>
        new(
        [
            Input(Items.StaminaPotion4, StaminaPotionsPerCatch / experiencePerCatch),
            Output(Items.GrimyGuamLeaf, 0.575m / experiencePerCatch),
            Output(Items.GrimyRanarrWeed, 0.246m / experiencePerCatch),
            Output(Items.GrimyIritLeaf, 0.221m / experiencePerCatch),
            Output(Items.GrimyAvantoe, 0.249m / experiencePerCatch),
            Output(Items.GrimyKwuarm, 0.327m / experiencePerCatch),
            Output(Items.GrimySnapdragon, 0.207m / experiencePerCatch),
            Output(Items.GrimyCadantine, 0.341m / experiencePerCatch),
            Output(Items.GrimyLantadyme, 0.327m / experiencePerCatch),
            Output(Items.GrimyDwarfWeed, 0.281m / experiencePerCatch),
            Output(Items.GrimyTorstol, 0.226m / experiencePerCatch),
            Output(Items.Numulite, NumulitePerCatch / experiencePerCatch)
        ]);

    private static class Items
    {
        public static readonly CatalogueItem StaminaPotion4 = new(12625, "Stamina potion(4)");
        public static readonly CatalogueItem GrimyGuamLeaf = new(199, "Grimy guam leaf");
        public static readonly CatalogueItem GrimyRanarrWeed = new(207, "Grimy ranarr weed");
        public static readonly CatalogueItem GrimyIritLeaf = new(209, "Grimy irit leaf");
        public static readonly CatalogueItem GrimyAvantoe = new(211, "Grimy avantoe");
        public static readonly CatalogueItem GrimyKwuarm = new(213, "Grimy kwuarm");
        public static readonly CatalogueItem GrimySnapdragon = new(3051, "Grimy snapdragon");
        public static readonly CatalogueItem GrimyCadantine = new(215, "Grimy cadantine");
        public static readonly CatalogueItem GrimyLantadyme = new(2485, "Grimy lantadyme");
        public static readonly CatalogueItem GrimyDwarfWeed = new(217, "Grimy dwarf weed");
        public static readonly CatalogueItem GrimyTorstol = new(219, "Grimy torstol");
        public static readonly CatalogueItem Numulite = new(21555, "Numulite");
    }
}
