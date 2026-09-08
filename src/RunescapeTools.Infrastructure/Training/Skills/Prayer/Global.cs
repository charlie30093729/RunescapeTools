using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer;

internal static class PrayerGlobal
{
    public const string OfferingLocationKey = "offering-location";
    private const string GildedAltar = "gilded-altar";
    private const string ChaosAltar = "chaos-altar";
    private const string OfferingAtBank = "offering-at-bank";
    private const string OfferingAtPrif = "offering-at-prif-agility";

    // Wiki Sinister Offering: 3 bones, 3x burial XP, 180 Magic XP, one blood + wrath rune.
    // 600 bank casts/hour includes banking beneath the 9-tick spell cooldown ceiling.
    private const decimal BonesPerCast = 3m;
    private const decimal OfferingExperienceMultiplier = 3m;
    private const decimal MagicExperiencePerCast = 180m;
    private const decimal BankCastsPerHour = 600m;

    // Calibrated to the supplied ~633-hour frost-bone 0-200m benchmark, not a measured lap.
    private const decimal PrifBonesPerLap = 24m;
    private const decimal PrifSecondsPerLapIncludingBanking = 82m;
    private const decimal PrifAgilityExperiencePerLap = 1_340.6m;
    private const decimal PrifExpectedShardsPerLap = 0.94m;
    private const decimal DivinePotionsPerShard = 2.5m;

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    OfferingLocationKey,
                    "Offering location",
                    TrainingConfigurationOptionKind.Choice,
                    GildedAltar,
                    "Select where the chosen bones are offered.",
                    [
                        new TrainingConfigurationChoice(
                            GildedAltar,
                            "Gilded altar"),
                        new TrainingConfigurationChoice(
                            ChaosAltar,
                            "Chaos altar"),
                        new TrainingConfigurationChoice(
                            OfferingAtBank,
                            "Offering at a bank"),
                        new TrainingConfigurationChoice(
                            OfferingAtPrif,
                            "Offering at Prif agility")
                    ])
            ]),
            ConfigureMethod,
            additionalMarketItemIds:
            [
                Items.BloodRune.Id, Items.WrathRune.Id,
                Items.SuperCombatPotion4.Id, Items.DivineSuperCombatPotion4.Id
            ]);

    public static PrayerSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration
                     ?? Configurator.Definition.Normalize();
        return new PrayerSettings(values.GetChoice(OfferingLocationKey));
    }

    public static bool UsesChaosAltar(PrayerSettings settings) =>
        string.Equals(settings.OfferingLocation, ChaosAltar, StringComparison.OrdinalIgnoreCase);

    public static string LocationName(PrayerSettings settings) =>
        UsesChaosAltar(settings) ? "Chaos Altar" : "Gilded Altar";

    public static bool UsesOfferingSpell(PrayerSettings settings) =>
        settings.OfferingLocation is OfferingAtBank or OfferingAtPrif;

    public static TrainingMethodDefinition CreateOfferingMethod(
        string id,
        string name,
        CatalogueItem bones,
        decimal burialExperience,
        PrayerSettings settings,
        long unlockExperience = 0)
    {
        var prif = settings.OfferingLocation == OfferingAtPrif;
        var experiencePerBone = burialExperience * OfferingExperienceMultiplier;
        var experiencePerCast = experiencePerBone * BonesPerCast;
        var experiencePerLap = experiencePerBone * PrifBonesPerLap;
        var rate = prif
            ? experiencePerLap * 3_600m / PrifSecondsPerLapIncludingBanking
            : experiencePerCast * BankCastsPerHour;
        List<TrainingResourceFlow> resources =
        [
            Input(bones, 1m / experiencePerBone),
            Input(Items.BloodRune, 1m / experiencePerCast),
            Input(Items.WrathRune, 1m / experiencePerCast)
        ];
        List<TrainingExperienceFlow> secondaryExperience =
        [
            new("Magic", MagicExperiencePerCast / experiencePerCast)
        ];
        if (prif)
        {
            var shardsPerExperience = PrifExpectedShardsPerLap / experiencePerLap;
            resources.Add(new TrainingResourceFlow(
                Items.CrystalShard.Id, Items.CrystalShard.Name, shardsPerExperience,
                TrainingFlowDirection.Output, SubjectToGeTax: false, RequiresMarketPrice: false));
            resources.Add(Input(Items.SuperCombatPotion4, shardsPerExperience * DivinePotionsPerShard));
            resources.Add(Output(Items.DivineSuperCombatPotion4, shardsPerExperience * DivinePotionsPerShard));
            secondaryExperience.Add(new("Agility", PrifAgilityExperiencePerLap / experiencePerLap));
        }

        var band = Band(unlockExperience, rate,
            $"{name} - Sinister Offering {(prif ? "at Prif agility" : "at a bank")}",
            new TrainingEconomics(resources), experienceOutputs: secondaryExperience);
        // Superior bones require 70 Prayer even with Sinister Offering. Preserve a valid
        // lower-level route using dragon bones at the same selected offering location.
        var bands = unlockExperience > 0
            ? Methods.DragonBones.Create(settings).Bands.Append(band).ToArray()
            : new[] { band };
        return new TrainingMethodDefinition(id, name, bands,
            "Sinister Offering requires 92 Magic, A Kingdom Divided, and the Arceuus spellbook. " +
            "Each full cast consumes three bones, one blood rune, and one wrath rune; grants 3x " +
            "burial Prayer XP and 180 Magic XP. No Zealot's robes or rune-saving equipment is assumed. " +
            (prif
                ? "Prif assumes Song of the Elves, at least 75 Agility, and an efficient high-level " +
                  "route: 24 bones (eight casts) per lap, 82 seconds including banking, 1,340.6 Agility " +
                  "XP and 0.94 expected crystal shards per lap. The lap-time estimate reproduces the " +
                  "reviewed ~633-hour frost-bone benchmark; lower-level failure rates are not modelled. " +
                  "All shards are valued through divine super combat potion(4) conversion at 2.5 " +
                  "potions per shard (97 Herblore); potion inputs buy high, outputs sell low after GE tax. " +
                  "Conversion time, Herblore XP, and run-energy supplies are excluded. "
                : "The bank default is 600 full casts/hour including banking (1,800 bones/hour). ") +
            "Personal XP/hour overrides change throughput and hours, not the materials or secondary " +
            "XP per Prayer XP. Secondary XP is a projection, not a change to the loaded profile. " +
            (unlockExperience > 0 ? "Dragon bones are used until level 70 Prayer. " : string.Empty),
            UseStableDisplayName: true);
    }

    private static TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues values)
    {
        var settings = ResolveSettings(values);
        return method.Id switch
        {
            "superior-dragon-bones" => Methods.SuperiorDragonBones.Create(settings),
            "dragon-bones" => Methods.DragonBones.Create(settings),
            "frost-dragon-bones" => Methods.FrostDragonBones.Create(settings),
            _ => method
        };
    }

    internal readonly record struct PrayerSettings(string OfferingLocation);

    private static class Items
    {
        public static readonly CatalogueItem BloodRune = new(565, "Blood rune");
        public static readonly CatalogueItem WrathRune = new(21880, "Wrath rune");
        public static readonly CatalogueItem CrystalShard = new(23962, "Crystal shard");
        public static readonly CatalogueItem SuperCombatPotion4 = new(12695, "Super combat potion(4)");
        public static readonly CatalogueItem DivineSuperCombatPotion4 = new(23685, "Divine super combat potion(4)");
    }
}
