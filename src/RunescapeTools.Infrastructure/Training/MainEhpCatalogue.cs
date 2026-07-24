using RunescapeTools.Application.Training;
using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training;

public sealed class MainEhpCatalogue : IEhpCatalogue
{
    private const int SuperiorDragonBonesId = 22124;
    private const int RawSummerPieId = 7216;
    private const int SummerPieId = 7218;
    private const int AstralRuneId = 9075;
    private const int BlackDragonLeatherId = 2509;
    private const int BlackDhideBodyId = 2503;
    private const int GoldOreId = 444;
    private const int GoldBarId = 2357;
    private const int StaminaPotion4Id = 12625;
    private const int ToadflaxPotionUnfinishedId = 3002;
    private const int CrushedNestId = 6693;
    private const int SaradominBrew3Id = 6687;
    private const int AmethystDartTipId = 25853;
    private const int FeatherId = 314;
    private const int AmethystDartId = 25849;
    private const int RosewoodLogsId = 32910;
    private const decimal EffectiveChaosAltarXpPerSuperiorBone = 1_050m;
    private const decimal SummerPieCookingXp = 260m;
    private const decimal BlackDhideBodyCraftingXp = 258m;
    private const decimal GoldBarSmithingXp = 56.2m;
    private const decimal SaradominBrewHerbloreXp = 180m;
    private const decimal AmethystDartFletchingXp = 21m;
    private const decimal RosewoodLogBowFiremakingXp = 420m;
    private const decimal BlastFurnaceGpPerHour = 72_000m;
    private const decimal BlastFurnaceUnder60GpPerHour = 87_000m;
    private const decimal StaminaPotion4PerHour = 10m;
    private const int OakPlankId = 8778;
    private const int MahoganyPlankId = 8782;
    private const decimal DemonButlerGpPerTrip = 10_000m / 8m;
    private const decimal DemonButlerCapacity = 24m;

    public string Version => "OSRS training catalogue 2026-07";

    public DateOnly VerifiedOn => new(2026, 7, 24);

    public IReadOnlyList<TrainingSkillDefinition> Skills { get; } = CreateSkills();

    private static IReadOnlyList<TrainingSkillDefinition> CreateSkills() =>
    [
        Skill("Attack", Standalone("Nightmare Zone fallback", 115_000m),
            note: "Main EHP treats Attack as Slayer bonus XP. This editable standalone fallback keeps Slayer isolated."),
        Skill("Defence", B(0, 455_000, "Black chinchompas and cannon - defensive")),
        Skill("Strength", Standalone("Nightmare Zone fallback", 115_000m),
            note: "Main EHP treats Strength as Slayer bonus XP. This editable standalone fallback keeps Slayer isolated."),
        Skill("Hitpoints", Standalone("Combat training fallback", 90_000m),
            note: "Main EHP treats Hitpoints as zero-time combat XP; replace this editable fallback when session credits are enabled."),
        Skill("Ranged",
            B(0, 250_000, "Bonus XP from Slayer"),
            B(6_517_253, 330_000, "Bonus XP from Slayer"),
            B(13_034_431, 1_325_000, "Chinning maniacal monkeys")),
        Skill("Prayer",
            B(0, 1_670_000, "Dagannoth bones at the chaos altar"),
            B(
                737_627,
                2_000_000,
                "Superior dragon bones at the Chaos Altar",
                SuperiorDragonBoneEconomics())),
        Skill("Magic", Standalone("Ice Barrage fallback", 330_000m),
            note: "Main EHP treats Magic as zero-time. The rate is editable until the standalone method is fully reviewed."),
        Skill("Cooking",
            B(0, 172_800, "1t poison karambwan"), B(13_363, 519_100, "1t karambwan"),
            B(37_224, 591_600, "1t karambwan"), B(101_333, 663_600, "1t karambwan"),
            B(273_742, 735_700, "1t karambwan"), B(737_627, 808_000, "1t karambwan"),
            B(1_986_068, 880_400, "1t karambwan"), B(5_346_332, 948_100, "1t karambwan"),
            B(8_771_558, 490_000, "Bake Pie spell - summer pies", SummerPieEconomics())),
        Skill("Woodcutting",
            B(0, 29_000, "Quests and trees"), B(2_411, 56_000, "2t oaks"),
            B(22_406, 93_174, "1.5t teaks"), B(41_171, 114_728, "1.5t teaks"),
            B(111_945, 127_339, "1.5t teaks"), B(302_288, 172_507, "1.5t teaks"),
            B(814_445, 194_022, "1.5t teaks"), B(1_986_068, 207_636, "1.5t teaks"),
            B(5_346_332, 221_977, "1.5t teaks"), B(13_034_431, 235_000, "1.5t teaks")),
        Skill(
            "Fletching",
            B(0, 1_000_000, "Zero-time Fletching - rate only"),
            B(5_346_332, 1_000_000, "Amethyst darts", AmethystDartEconomics())),
        Skill("Fishing",
            B(0, 29_200, "Quests"), B(14_612, 46_592, "3t fly fishing"),
            B(75_127, 84_686, "Drift net fishing"), B(106_046, 97_867, "Drift net fishing"),
            B(229_685, 112_877, "Drift net fishing"), B(302_288, 128_082, "Drift net fishing"),
            B(593_234, 139_313, "Drift net fishing"), B(737_627, 132_800, "Drift net plus 2t swordfish and tuna")),
        Skill("Firemaking",
            B(0, 73_700, "Coloured logs"), B(22_406, 138_900, "Teak logs"),
            B(45_529, 184_250, "Arctic pine logs"), B(61_512, 198_990, "Maple logs"),
            B(101_333, 400_271, "Artefacts with firemaking"), B(273_742, 522_696, "Artefacts with firemaking"),
            B(1_210_421, 768_800, "Artefacts with firemaking"), B(5_346_332, 864_981, "Artefacts with firemaking"),
            B(13_034_431, 623_700, "Rosewood logs - bow burning", RosewoodLogEconomics())),
        Skill("Crafting",
            B(0, 37_000, "Leather items"), B(4_470, 139_000, "Sapphires"),
            B(9_730, 187_650, "Emeralds"), B(20_224, 236_300, "Rubies"),
            B(50_339, 298_850, "Diamonds"), B(368_599, 335_230, "Green d'hide bodies"),
            B(814_445, 378_490, "Blue d'hide bodies"), B(1_475_581, 421_740, "Red d'hide bodies"),
            B(2_951_373, 465_000, "Black dragonhide bodies", BlackDhideBodyEconomics())),
        Skill("Smithing",
            B(0, 46_500, "Quests"),
            B(
                37_224,
                380_000,
                "Solo Blast Furnace gold",
                SoloBlastFurnaceGoldEconomics(BlastFurnaceUnder60GpPerHour)),
            B(
                273_742,
                380_000,
                "Solo Blast Furnace gold",
                SoloBlastFurnaceGoldEconomics(BlastFurnaceGpPerHour)),
            B(
                13_034_431,
                410_000,
                "Solo Blast Furnace gold",
                SoloBlastFurnaceGoldEconomics(BlastFurnaceGpPerHour))),
        Skill("Mining",
            B(0, 20_000, "Quests"), B(35_025, 50_000, "Prospector and celestial ring"),
            B(393_485, 106_540, "3t granite"), B(1_210_421, 112_166, "3t granite"),
            B(3_258_594, 116_760, "3t granite"), B(8_771_558, 119_438, "3t granite"),
            B(13_034_431, 126_000, "3t granite")),
        Skill("Herblore",
            B(0, 11_100, "Quests"), B(8_025, 218_750, "Serum 207s"),
            B(123_660, 293_750, "Super energies"), B(166_636, 312_500, "Super strengths"),
            B(368_599, 356_250, "Super restores"), B(496_254, 375_000, "Super defences"),
            B(668_051, 393_750, "Antifire potions"), B(899_257, 406_250, "Ranging potions"),
            B(1_336_443, 431_250, "Magic potions"), B(1_475_581, 535_500, "1t stamina potions"),
            B(2_192_818, 450_000, "Saradomin brews", SaradominBrewEconomics())),
        Skill("Agility",
            B(0, 15_100, "Quests"), B(75_127, 35_000, "Wilderness Agility Course"),
            B(123_660, 45_000, "Hallowed Sepulchre"), B(333_804, 56_300, "Hallowed Sepulchre"),
            B(899_257, 68_900, "Hallowed Sepulchre"), B(2_421_087, 79_700, "Hallowed Sepulchre"),
            B(6_517_253, 102_000, "Hallowed Sepulchre with brews")),
        Skill("Thieving",
            B(0, 15_000, "Quests and fruit stalls"), B(61_512, 80_000, "Blackjacking"),
            B(91_721, 247_014, "Artefacts with firemaking"), B(295_921, 291_617, "Artefacts with firemaking"),
            B(1_322_779, 340_358, "Artefacts with firemaking"), B(4_814_243, 378_482, "Artefacts with firemaking"),
            B(10_999_977, 374_790, "Artefacts with Bake Pie"), B(13_034_431, 381_266, "Artefacts with Bake Pie")),
        Skill("Slayer",
            B(0, 5_000, "Efficient Slayer"), B(37_224, 12_000, "Efficient Slayer"),
            B(101_333, 40_000, "Efficient Slayer"), B(449_428, 74_250, "Efficient Slayer"),
            B(1_986_068, 79_000, "Efficient Slayer"), B(3_258_594, 86_500, "Efficient Slayer"),
            B(5_346_332, 87_000, "Efficient Slayer"), B(7_195_629, 93_000, "Efficient Slayer"),
            B(13_034_431, 110_900, "Efficient Slayer")),
        Skill("Farming",
            B(0, 16_000, "Quests"), B(32_500, 364_000, "Tree runs"),
            B(61_512, 575_000, "Tree runs"), B(166_636, 841_000, "Tree runs"),
            B(273_742, 1_222_000, "Tree runs"), B(605_032, 1_428_000, "Tree runs"),
            B(1_210_421, 2_063_000, "Tree runs"), B(2_192_818, 2_475_000, "Tree runs"),
            B(3_258_594, 2_611_000, "Tree runs"), B(6_517_253, 2_669_000, "Tree runs")),
        Skill("Runecraft",
            B(0, 13_600, "Quests"), B(33_210, 45_000, "Guardians of the Rift rewards"),
            B(1_210_421, 75_400, "Solo mud runes"), B(3_258_594, 106_100, "Solo mud runes"),
            B(13_034_431, 200_000, "2+1 aether runes")),
        Skill("Hunter",
            B(0, 30_000, "Varrock museum and birdhouses"), B(2_107, 83_000, "Oak birdhouses"),
            B(7_028, 110_000, "Willow birdhouses"), B(20_224, 138_000, "Teak birdhouses"),
            B(55_649, 215_112, "Drift net fishing"), B(91_721, 268_770, "Drift net fishing"),
            B(184_040, 293_310, "Drift net fishing"), B(343_551, 322_424, "Drift net fishing"),
            B(737_627, 350_697, "Drift net fishing"), B(933_979, 275_000, "Drift net / black chinchompas")),
        Construction(),
        Skill("Sailing",
            B(0, 27_000, "Quests, Tears of Guthix and Tempor Tantrum"),
            B(101_333, 45_000, "1.5t large shipwrecks"), B(166_636, 100_000, "The Jubbly Jive and charting"),
            B(899_257, 220_000, "The Gwenith Glide - camphor hull"),
            B(4_842_295, 255_000, "The Gwenith Glide - rosewood hull with Spin Flax"))
    ];

    private static TrainingEconomics SuperiorDragonBoneEconomics() =>
        new(
            [
                Input(
                    SuperiorDragonBonesId,
                    "Superior dragon bones",
                    1m / EffectiveChaosAltarXpPerSuperiorBone)
            ]);

    private static TrainingEconomics SummerPieEconomics() =>
        new(
            [
                Input(RawSummerPieId, "Raw summer pie", 1m / SummerPieCookingXp),
                Input(AstralRuneId, "Astral rune", 1m / SummerPieCookingXp),
                Output(SummerPieId, "Summer pie", 1m / SummerPieCookingXp)
            ]);

    private static TrainingEconomics BlackDhideBodyEconomics() =>
        new(
            [
                Input(BlackDragonLeatherId, "Black dragon leather", 3m / BlackDhideBodyCraftingXp),
                Output(BlackDhideBodyId, "Black d'hide body", 1m / BlackDhideBodyCraftingXp)
            ]);

    private static TrainingEconomics SoloBlastFurnaceGoldEconomics(decimal fixedGpPerHour) =>
        new(
            [
                Input(GoldOreId, "Gold ore", 1m / GoldBarSmithingXp),
                Input(
                    StaminaPotion4Id,
                    "Stamina potion(4)",
                    0m,
                    quantityPerHour: StaminaPotion4PerHour),
                Output(GoldBarId, "Gold bar", 1m / GoldBarSmithingXp)
            ],
            FixedGpPerHour: fixedGpPerHour);

    private static TrainingEconomics SaradominBrewEconomics() =>
        new(
            [
                Input(ToadflaxPotionUnfinishedId, "Toadflax potion (unf)", 1m / SaradominBrewHerbloreXp),
                Input(CrushedNestId, "Crushed nest", 1m / SaradominBrewHerbloreXp),
                Output(SaradominBrew3Id, "Saradomin brew(3)", 1m / SaradominBrewHerbloreXp)
            ]);

    private static TrainingEconomics AmethystDartEconomics() =>
        new(
            [
                Input(AmethystDartTipId, "Amethyst dart tip", 1m / AmethystDartFletchingXp),
                Input(FeatherId, "Feather", 1m / AmethystDartFletchingXp),
                Output(AmethystDartId, "Amethyst dart", 1m / AmethystDartFletchingXp)
            ]);

    private static TrainingEconomics RosewoodLogEconomics() =>
        new([Input(RosewoodLogsId, "Rosewood logs", 1m / RosewoodLogBowFiremakingXp)]);

    private static TrainingSkillDefinition Construction()
    {
        var oakEconomics = PlankEconomics(OakPlankId, "Oak plank", 60m);
        var mahoganyEconomics = PlankEconomics(MahoganyPlankId, "Mahogany plank", 140m);
        return Skill(
            "Construction",
            B(0, 54_700, "Low-level furniture"),
            B(18_247, 200_000, "Oak larders", oakEconomics),
            B(37_224, 290_000, "Mahogany bookcases", mahoganyEconomics),
            B(123_660, 950_000, "Mahogany tables", mahoganyEconomics),
            B(1_475_581, 1_070_000, "Mahogany benches", mahoganyEconomics),
            B(13_034_431, 1_440_000, "2t mahogany flatpacks", mahoganyEconomics));
    }

    private static TrainingEconomics PlankEconomics(int itemId, string name, decimal experiencePerPlank) =>
        new(
            [new TrainingResourceFlow(itemId, name, 1m / experiencePerPlank, TrainingFlowDirection.Input)],
            DemonButlerGpPerTrip / DemonButlerCapacity / experiencePerPlank);

    private static TrainingResourceFlow Input(
        int itemId,
        string name,
        decimal quantityPerExperience,
        decimal quantityPerHour = 0m) =>
        new(
            itemId,
            name,
            quantityPerExperience,
            TrainingFlowDirection.Input,
            QuantityPerHour: quantityPerHour);

    private static TrainingResourceFlow Output(
        int itemId,
        string name,
        decimal quantityPerExperience) =>
        new(itemId, name, quantityPerExperience, TrainingFlowDirection.Output);

    private static TrainingRateBand Standalone(string method, decimal rate) => B(0, rate, method);

    private static TrainingSkillDefinition Skill(
        string name,
        TrainingRateBand band,
        string? note = null) => new(name, [band], Note: note);

    private static TrainingSkillDefinition Skill(string name, params TrainingRateBand[] bands) =>
        new(name, bands);

    private static TrainingRateBand B(
        long startExperience,
        decimal experiencePerHour,
        string method,
        TrainingEconomics? economics = null) =>
        new(startExperience, experiencePerHour, method, economics);
}
