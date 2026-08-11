using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft;

internal static class RunecraftGlobal
{
    public const string RaimentsOfTheEyeKey = "raiments-of-the-eye";
    public const long RunecraftCapeExperience = 13_034_431;
    private const decimal FullRaimentsBonusPerTenRunes = 6m;
    private const decimal DarkAltarBindingExperiencePerFragment = 0.625m;
    private const decimal ArceuusFragmentsPerCraft = 100m;

    public const string Note =
        "Full Raiments of the Eye follow the saved Runecraft configuration. The outfit adds 60% rune " +
        "output but no Runecraft XP; aether bonus runes consume matching aether catalysts. Magic Imbue, " +
        "binding-necklace disposal, and pouch repair are priced where applicable. Pouch-repair runes " +
        "automatically stop at 13,034,431 XP when the Runecraft cape prevents further degradation. " +
        "Reusable equipment and untradeable unlock costs are excluded.";

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    RaimentsOfTheEyeKey,
                    "Full Raiments of the Eye",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.TrueString,
                    "Create 60% more runes without changing XP/hour or essence consumption.")
            ]),
            ConfigureMethod);

    public static RunecraftSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration ?? Configurator.Definition.Normalize();
        return new RunecraftSettings(values.GetToggle(RaimentsOfTheEyeKey));
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
        TrainingConfigurationValues values)
    {
        var settings = ResolveSettings(values);
        return method.Id switch
        {
            "main-ehp" => Methods.SoloMudRunes.Create(settings),
            "solo-lava-runes" => Methods.SoloLavaRunes.Create(settings),
            "solo-aether-runes" => Methods.SoloAetherRunes.Create(settings),
            "achievement-cape-double-nature-runes" => Methods.AchievementCapeNatureRunes.Create(settings),
            "arceuus-blood-runes" => Methods.ArceuusBloodRunes.Create(settings),
            "arceuus-soul-runes" => Methods.ArceuusSoulRunes.Create(settings),
            _ => method
        };
    }

    internal readonly record struct RunecraftSettings(bool RaimentsOfTheEye);

    private static class Items
    {
        public static readonly CatalogueItem PureEssence = new(7936, "Pure essence");
        public static readonly CatalogueItem BindingNecklace = new(5521, "Binding necklace");
        public static readonly CatalogueItem AstralRune = new(9075, "Astral rune");
        public static readonly CatalogueItem AirRune = new(556, "Air rune");
        public static readonly CatalogueItem CosmicRune = new(564, "Cosmic rune");
    }
}
