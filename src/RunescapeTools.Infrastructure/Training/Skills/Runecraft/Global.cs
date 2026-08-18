using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft;

internal static class RunecraftGlobal
{
    public const string RaimentsOfTheEyeKey = "raiments-of-the-eye";
    public const string ArdougneMediumDiaryKey = "ardougne-medium-diary";
    public const string UseDaeyaltEssenceKey = "use-daeyalt-essence";
    public const string DaeyaltEssenceQuantityKey = "daeyalt-essence-quantity";
    public const long RunecraftCapeExperience = 13_034_431;
    private const decimal DaeyaltExperienceMultiplier = 1.5m;
    private const decimal FullRaimentsBonusPerTenRunes = 6m;
    private const decimal DarkAltarBindingExperiencePerFragment = 0.625m;
    private const decimal ArceuusFragmentsPerCraft = 100m;

    public const string Note =
        "Full Raiments of the Eye follow the saved Runecraft configuration. The outfit adds 60% rune " +
        "output but no Runecraft XP; aether bonus runes consume matching aether catalysts. Magic Imbue, " +
        "binding-necklace disposal, and pouch repair are priced where applicable. Pouch-repair runes " +
        "automatically stop at 13,034,431 XP when the Runecraft cape prevents further degradation. " +
        "Configured Daeyalt essence replaces pure essence in eligible segments and grants 50% bonus XP; " +
        "dark-essence Arceuus segments are unaffected. The saved Ardougne medium diary setting applies " +
        "only to Ourania Altar output. Reusable equipment and untradeable unlock costs are excluded.";

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    RaimentsOfTheEyeKey,
                    "Full Raiments of the Eye",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.TrueString,
                    "Create 60% more runes without changing XP/hour or essence consumption."),
                new TrainingConfigurationOption(
                    ArdougneMediumDiaryKey,
                    "Ardougne medium diary",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.TrueString,
                    "Apply the diary's type-specific chance to create a bonus rune at the Ourania Altar. " +
                    "This changes rune output but not XP/hour."),
                new TrainingConfigurationOption(
                    UseDaeyaltEssenceKey,
                    "Use Daeyalt essence",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.FalseString,
                    "Replace pure essence with owned Daeyalt essence for 50% more Runecraft XP. " +
                    "This does not apply to dark essence fragments."),
                new TrainingConfigurationOption(
                    DaeyaltEssenceQuantityKey,
                    "Daeyalt essence available",
                    TrainingConfigurationOptionKind.Number,
                    string.Empty,
                    "Optional. Leave blank for unlimited Daeyalt essence; otherwise the route returns " +
                    "to pure essence after using this amount.",
                    MinimumValue: 0m,
                    MaximumValue: 1_000_000_000m,
                    AllowsEmpty: true,
                    WholeNumbersOnly: true)
            ]),
            ConfigureMethod);

    public static RunecraftSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration ?? Configurator.Definition.Normalize();
        return new RunecraftSettings(
            values.GetToggle(RaimentsOfTheEyeKey),
            values.GetToggle(ArdougneMediumDiaryKey),
            values.GetToggle(UseDaeyaltEssenceKey),
            values.GetOptionalWholeNumber(DaeyaltEssenceQuantityKey));
    }

    public static decimal OutputPerLap(
        decimal baseRunesPerLap,
        RunecraftSettings settings) =>
        settings.RaimentsOfTheEye
            ? baseRunesPerLap + (decimal.Floor(baseRunesPerLap / 10m) * FullRaimentsBonusPerTenRunes)
            : baseRunesPerLap;

    public static IReadOnlyList<TrainingRateBand> CreateBaseBands() =>
    [
        Band(0, 13_600m, "Quests"),
        Band(33_210, 45_000m, "Guardians of the Rift rewards")
    ];

    public static TrainingRateBand CreateArceuusRuneBand(
        long startExperience,
        decimal experiencePerHour,
        decimal altarExperiencePerFragment,
        string method,
        CatalogueItem outputRune,
        RunecraftSettings settings)
    {
        var experiencePerFragment =
            altarExperiencePerFragment + DarkAltarBindingExperiencePerFragment;
        var outputPerCraft = OutputPerLap(ArceuusFragmentsPerCraft, settings);
        return Band(
            startExperience,
            experiencePerHour,
            method,
            new TrainingEconomics(
            [
                Output(
                    outputRune,
                    outputPerCraft / (ArceuusFragmentsPerCraft * experiencePerFragment))
            ]));
    }

    public static List<TrainingResourceFlow> CreateCommonResources(
        decimal experiencePerLap,
        decimal essencePerLap,
        decimal bindingNecklacesPerLap,
        decimal magicImbueAstralRunesPerLap,
        decimal pouchRepairsPerLap)
    {
        var astralRunesPerLap = magicImbueAstralRunesPerLap + pouchRepairsPerLap;
        var airRunesPerLap = pouchRepairsPerLap * 2m;
        var cosmicRunesPerLap = pouchRepairsPerLap;
        var resources = new List<TrainingResourceFlow>
        {
            Input(Items.PureEssence, essencePerLap / experiencePerLap)
        };
        if (bindingNecklacesPerLap > 0m)
            resources.Add(Input(Items.BindingNecklace, bindingNecklacesPerLap / experiencePerLap));
        if (astralRunesPerLap > 0m)
            resources.Add(Input(Items.AstralRune, astralRunesPerLap / experiencePerLap));
        if (airRunesPerLap > 0m)
            resources.Add(Input(Items.AirRune, airRunesPerLap / experiencePerLap));
        if (cosmicRunesPerLap > 0m)
            resources.Add(Input(Items.CosmicRune, cosmicRunesPerLap / experiencePerLap));
        return resources;
    }

    private static TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues values,
        TrainingCalculationContext context)
    {
        var settings = ResolveSettings(values);
        var configured = method.Id switch
        {
            "main-ehp" => Methods.SoloMudRunes.Create(settings),
            "solo-lava-runes" => Methods.SoloLavaRunes.Create(settings),
            "solo-aether-runes" => Methods.SoloAetherRunes.Create(settings),
            "achievement-cape-double-nature-runes" => Methods.AchievementCapeNatureRunes.Create(settings),
            "arceuus-blood-runes" => Methods.ArceuusBloodRunes.Create(settings),
            "arceuus-soul-runes" => Methods.ArceuusSoulRunes.Create(settings),
            "ourania-altar-zmi" => Methods.OuraniaAltarZmi.Create(settings),
            _ => method
        };

        return ApplyDaeyaltEssence(configured, settings, context);
    }

    private static TrainingMethodDefinition ApplyDaeyaltEssence(
        TrainingMethodDefinition method,
        RunecraftSettings settings,
        TrainingCalculationContext context)
    {
        if (!settings.UseDaeyaltEssence || settings.DaeyaltEssenceQuantity is 0)
            return method;

        decimal? remainingEssence = settings.DaeyaltEssenceQuantity;
        var ordered = method.Bands.OrderBy(band => band.StartExperience).ToArray();
        var bands = new List<TrainingRateBand>(ordered.Length * 2);

        for (var index = 0; index < ordered.Length; index++)
        {
            var band = ordered[index];
            bands.Add(band);

            var nextStart = index + 1 < ordered.Length
                ? ordered[index + 1].StartExperience
                : TrainingPlanCalculator.MaximumExperience;
            var segmentStart = Math.Max(context.StartExperience, band.StartExperience);
            var segmentEnd = Math.Min(context.TargetExperience, nextStart);
            if (segmentEnd <= segmentStart || remainingEssence is <= 0m)
                continue;

            var pureEssence = band.Economics?.Resources.FirstOrDefault(resource =>
                resource.ItemId == Items.PureEssence.Id
                && resource.Direction == TrainingFlowDirection.Input
                && resource.QuantityPerExperience > 0m);
            if (pureEssence is null)
                continue;

            var segmentExperience = segmentEnd - segmentStart;
            var availableDaeyaltExperience = remainingEssence.HasValue
                ? decimal.Floor(
                    remainingEssence.Value
                    / pureEssence.QuantityPerExperience
                    * DaeyaltExperienceMultiplier)
                : segmentExperience;
            var daeyaltExperience = (long)Math.Min(
                segmentExperience,
                Math.Max(0m, availableDaeyaltExperience));
            if (daeyaltExperience <= 0)
                continue;

            bands.Add(CreateDaeyaltBand(band, segmentStart));
            if (daeyaltExperience < segmentExperience)
            {
                bands.Add(band with
                {
                    StartExperience = segmentStart + daeyaltExperience
                });
            }

            if (remainingEssence.HasValue)
            {
                remainingEssence = Math.Max(
                    0m,
                    remainingEssence.Value
                    - daeyaltExperience
                    * pureEssence.QuantityPerExperience
                    / DaeyaltExperienceMultiplier);
            }
        }

        return method with { Bands = bands };
    }

    private static TrainingRateBand CreateDaeyaltBand(
        TrainingRateBand band,
        long startExperience)
    {
        var economics = band.Economics;
        return band with
        {
            StartExperience = startExperience,
            ExperiencePerHour = band.ExperiencePerHour * DaeyaltExperienceMultiplier,
            ConfigurationRateMultiplier =
                band.ConfigurationRateMultiplier * DaeyaltExperienceMultiplier,
            Method = $"{band.Method} - Daeyalt essence",
            Economics = economics is null
                ? null
                : economics with
                {
                    Resources = economics.Resources.Select(resource =>
                    {
                        var scaled = resource with
                        {
                            QuantityPerExperience =
                                resource.QuantityPerExperience / DaeyaltExperienceMultiplier
                        };
                        return resource.ItemId == Items.PureEssence.Id
                               && resource.Direction == TrainingFlowDirection.Input
                            ? scaled with
                            {
                                ItemId = Items.DaeyaltEssence.Id,
                                Name = Items.DaeyaltEssence.Name,
                                SubjectToGeTax = false,
                                RequiresMarketPrice = false
                            }
                            : scaled;
                    }).ToArray(),
                    FixedGpPerExperience =
                        economics.FixedGpPerExperience / DaeyaltExperienceMultiplier,
                    FixedGpOutputPerExperience =
                        economics.FixedGpOutputPerExperience / DaeyaltExperienceMultiplier
                }
        };
    }

    internal readonly record struct RunecraftSettings(
        bool RaimentsOfTheEye,
        bool ArdougneMediumDiary,
        bool UseDaeyaltEssence,
        long? DaeyaltEssenceQuantity);

    private static class Items
    {
        public static readonly CatalogueItem PureEssence = new(7936, "Pure essence");
        public static readonly CatalogueItem DaeyaltEssence = new(24704, "Daeyalt essence");
        public static readonly CatalogueItem BindingNecklace = new(5521, "Binding necklace");
        public static readonly CatalogueItem AstralRune = new(9075, "Astral rune");
        public static readonly CatalogueItem AirRune = new(556, "Air rune");
        public static readonly CatalogueItem CosmicRune = new(564, "Cosmic rune");
    }
}
