using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class OuraniaAltarZmi
{
    private const decimal FullRaimentsMultiplier = 1.6m;
    private const decimal OuraniaExperienceMultiplier = 1.7m;
    private const decimal MindRunesPerBank = 20m;
    private const decimal AstralRunesPerTeleport = 2m;
    private const decimal LawRunesPerTeleport = 1m;

    private static readonly RuneDefinition[] Runes =
    [
        new(Items.SoulRune, 11m, 0.10m),
        new(Items.BloodRune, 10.5m, 0.15m),
        new(Items.DeathRune, 10m, 0.175m),
        new(Items.LawRune, 9.5m, 0.20m),
        new(Items.NatureRune, 9m, 0.225m),
        new(Items.AstralRune, 8.7m, 0.25m),
        new(Items.ChaosRune, 8.5m, 0.25m),
        new(Items.CosmicRune, 8m, 0.25m),
        new(Items.BodyRune, 7.5m, 0.25m),
        new(Items.FireRune, 7m, 0.25m),
        new(Items.EarthRune, 6.5m, 0.25m),
        new(Items.WaterRune, 6m, 0.25m),
        new(Items.MindRune, 5.5m, 0.25m),
        new(Items.AirRune, 5m, 0.25m)
    ];

    private static readonly RuneDistribution Level1 = Distribution(
        2, 7, 15, 30, 60, 105, 165, 250, 400, 700, 1_300, 2_500, 5_000, 10_000);
    private static readonly RuneDistribution Level10 = Distribution(
        3, 9, 21, 45, 85, 145, 225, 400, 1_000, 2_200, 4_600, 6_700, 8_500, 10_000);
    private static readonly RuneDistribution Level20 = Distribution(
        8, 23, 55, 110, 220, 430, 850, 1_650, 3_250, 4_750, 6_150, 7_500, 8_800, 10_000);
    private static readonly RuneDistribution Level30 = Distribution(
        20, 60, 120, 250, 500, 1_000, 2_000, 4_000, 5_300, 6_500, 7_600, 8_500, 9_300, 10_000);
    private static readonly RuneDistribution Level40 = Distribution(
        40, 120, 240, 500, 1_000, 2_000, 4_000, 5_500, 6_500, 7_300, 8_050, 8_750, 9_400, 10_000);
    private static readonly RuneDistribution Level50 = Distribution(
        80, 250, 600, 1_300, 2_650, 4_150, 5_250, 6_250, 7_000, 7_700, 8_350, 8_950, 9_500, 10_000);
    private static readonly RuneDistribution Level60 = Distribution(
        100, 300, 700, 1_500, 3_050, 4_450, 5_500, 6_450, 7_200, 7_900, 8_500, 9_050, 9_550, 10_000);
    private static readonly RuneDistribution Level70 = Distribution(
        200, 700, 1_700, 3_500, 5_000, 6_200, 7_100, 7_800, 8_300, 8_700, 9_100, 9_400, 9_700, 10_000);
    private static readonly RuneDistribution Level80 = Distribution(
        400, 1_000, 2_450, 3_900, 5_250, 6_300, 7_100, 7_800, 8_400, 8_900, 9_300, 9_600, 9_800, 10_000);
    private static readonly RuneDistribution Level90 = Distribution(
        650, 1_650, 3_300, 4_750, 6_100, 7_100, 7_800, 8_400, 8_900, 9_300, 9_600, 9_800, 9_900, 10_000);
    private static readonly RuneDistribution Level99 = Distribution(
        900, 2_200, 3_750, 5_200, 6_550, 7_500, 8_100, 8_600, 9_000, 9_300, 9_600, 9_800, 9_900, 10_000);

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "ourania-altar-zmi",
            "Ourania Altar (ZMI)",
            [
                CreateBand(0, 20_423m, 29m, 0m, Level1, settings),
                CreateBand(1_154, 22_881m, 29m, 0m, Level10, settings),
                CreateBand(4_470, 24_665m, 29m, 0m, Level20, settings),
                CreateBand(7_842, 28_797m, 34m, 0.0222m, Level20, settings),
                CreateBand(13_363, 31_158m, 34m, 0.0222m, Level30, settings),
                CreateBand(37_224, 32_758m, 34m, 0.0222m, Level40, settings),
                CreateBand(101_333, 42_077m, 42m, 0.0345m, Level50, settings),
                CreateBand(273_742, 42_577m, 42m, 0.0345m, Level60, settings),
                CreateBand(737_627, 45_576m, 42m, 0.0345m, Level70, settings),
                CreateBand(1_210_421, 57_270m, 53m, 0.1m, Level70, settings),
                CreateBand(1_986_068, 58_487m, 53m, 0.1m, Level80, settings),
                CreateBand(3_258_594, 72_526m, 66m, 0.125m, Level80, settings),
                CreateBand(5_346_332, 74_716m, 66m, 0.125m, Level90, settings),
                CreateBand(RunecraftGlobal.RunecraftCapeExperience, 77_121m, 66m, 0m, Level99, settings)
            ],
            RunecraftGlobal.Note +
            " Ourania Altar rates use the Wiki's level-dependent rune distributions, 48-second efficient " +
            "laps, every available pouch, Ourania Teleport, and NPC Contact before level 99. Eniola is " +
            "paid with 20 mind runes per bank access. The equipped dust battlestaff supplies earth and air " +
            "runes; run-energy restoration, reusable gear, and quest unlocks are excluded. Daeyalt mining " +
            "time is not included.");

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        decimal essencePerLap,
        decimal pouchRepairsPerLap,
        RuneDistribution distribution,
        RunecraftGlobal.RunecraftSettings settings)
    {
        var experiencePerEssence = distribution.ExperiencePerEssence;
        var experiencePerLap = essencePerLap * experiencePerEssence;
        var resources = new List<TrainingResourceFlow>
        {
            Input(Items.PureEssence, 1m / experiencePerEssence),
            Input(Items.MindRune, MindRunesPerBank / experiencePerLap),
            Input(
                Items.AstralRune,
                (AstralRunesPerTeleport + pouchRepairsPerLap) / experiencePerLap),
            Input(Items.LawRune, LawRunesPerTeleport / experiencePerLap)
        };
        if (pouchRepairsPerLap > 0m)
            resources.Add(Input(Items.CosmicRune, pouchRepairsPerLap / experiencePerLap));

        var raimentsMultiplier = settings.RaimentsOfTheEye ? FullRaimentsMultiplier : 1m;
        for (var index = 0; index < Runes.Length; index++)
        {
            var rune = Runes[index];
            var diaryMultiplier = settings.ArdougneMediumDiary
                ? 1m + rune.ArdougneBonusChance
                : 1m;
            resources.Add(Output(
                rune.Item,
                distribution.Chances[index] * diaryMultiplier * raimentsMultiplier
                / experiencePerEssence));
        }

        return Band(
            startExperience,
            experiencePerHour,
            "Ourania Altar (ZMI)",
            new TrainingEconomics(resources));
    }

    private static RuneDistribution Distribution(params int[] cumulativeThresholds)
    {
        if (cumulativeThresholds.Length != Runes.Length || cumulativeThresholds[^1] != 10_000)
            throw new ArgumentException("A ZMI distribution must cover all runes through 10,000.");

        var chances = new decimal[cumulativeThresholds.Length];
        var previous = 0;
        decimal baseExperience = 0m;
        for (var index = 0; index < cumulativeThresholds.Length; index++)
        {
            var threshold = cumulativeThresholds[index];
            if (threshold < previous)
                throw new ArgumentException("ZMI distribution thresholds must be ordered.");

            chances[index] = (threshold - previous) / 10_000m;
            baseExperience += chances[index] * Runes[index].BaseExperience;
            previous = threshold;
        }

        return new RuneDistribution(chances, baseExperience * OuraniaExperienceMultiplier);
    }

    private sealed record RuneDistribution(
        IReadOnlyList<decimal> Chances,
        decimal ExperiencePerEssence);

    private readonly record struct RuneDefinition(
        CatalogueItem Item,
        decimal BaseExperience,
        decimal ArdougneBonusChance);

    private static class Items
    {
        public static readonly CatalogueItem PureEssence = new(7936, "Pure essence");
        public static readonly CatalogueItem AirRune = new(556, "Air rune");
        public static readonly CatalogueItem MindRune = new(558, "Mind rune");
        public static readonly CatalogueItem WaterRune = new(555, "Water rune");
        public static readonly CatalogueItem EarthRune = new(557, "Earth rune");
        public static readonly CatalogueItem FireRune = new(554, "Fire rune");
        public static readonly CatalogueItem BodyRune = new(559, "Body rune");
        public static readonly CatalogueItem CosmicRune = new(564, "Cosmic rune");
        public static readonly CatalogueItem ChaosRune = new(562, "Chaos rune");
        public static readonly CatalogueItem AstralRune = new(9075, "Astral rune");
        public static readonly CatalogueItem NatureRune = new(561, "Nature rune");
        public static readonly CatalogueItem LawRune = new(563, "Law rune");
        public static readonly CatalogueItem DeathRune = new(560, "Death rune");
        public static readonly CatalogueItem BloodRune = new(565, "Blood rune");
        public static readonly CatalogueItem SoulRune = new(566, "Soul rune");
    }
}
