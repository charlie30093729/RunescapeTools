using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Firemaking;

internal static class FiremakingGlobal
{
    public const string PyromancerOutfitKey = "pyromancer-outfit";
    public const string BonfireKey = "bonfire";
    private const decimal PyromancerMultiplier = 1.025m;
    private const decimal BowLogsPerHour = 1_485m;
    private const decimal AutomaticBonfireLogsPerHour = 665m;

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    PyromancerOutfitKey,
                    "Pyromancer outfit",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.TrueString,
                    "Apply the full outfit's 2.5% Firemaking XP bonus."),
                new TrainingConfigurationOption(
                    BonfireKey,
                    "Use a bonfire",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.FalseString,
                    "Use the low-effort automatic Forester's Campfire rate instead of normal burning.")
            ]),
            ConfigureMethod);

    public static FiremakingSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration
                     ?? Configurator.Definition.Normalize();
        return new FiremakingSettings(
            values.GetToggle(PyromancerOutfitKey),
            values.GetToggle(BonfireKey));
    }

    public static IReadOnlyList<TrainingRateBand> CreateBaseBands() =>
    [
        Band(0, 73_700m, "Coloured logs"),
        Band(22_406, 138_900m, "Teak logs"),
        Band(45_529, 184_250m, "Arctic pine logs"),
        Band(61_512, 198_990m, "Maple logs"),
        Band(101_333, 400_271m, "Artefacts with firemaking"),
        Band(273_742, 522_696m, "Artefacts with firemaking"),
        Band(1_210_421, 768_800m, "Artefacts with firemaking"),
        Band(5_346_332, 864_981m, "Artefacts with firemaking")
    ];

    public static decimal LogsPerHour(FiremakingSettings settings) =>
        settings.UseBonfire ? AutomaticBonfireLogsPerHour : BowLogsPerHour;

    public static TrainingMethodDefinition ApplyPyromancer(
        TrainingMethodDefinition method,
        FiremakingSettings settings) =>
        TrainingConfigurationTransforms.ApplyExperienceMultiplier(
            method,
            settings.PyromancerOutfit ? PyromancerMultiplier : 1m);

    private static TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues values)
    {
        var settings = ResolveSettings(values);
        return method.Id switch
        {
            "main-ehp" => Methods.RosewoodLogs.Create(settings),
            "redwood-logs" => Methods.RedwoodLogs.Create(settings),
            _ => method
        };
    }

    internal readonly record struct FiremakingSettings(
        bool PyromancerOutfit,
        bool UseBonfire);

}
