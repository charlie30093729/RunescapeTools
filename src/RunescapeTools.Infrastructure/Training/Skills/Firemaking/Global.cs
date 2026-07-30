using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Firemaking;

internal static class FiremakingGlobal
{
    public const string PyromancerOutfitKey = "pyromancer-outfit";
    public const string BonfireKey = "bonfire";
    private const decimal PyromancerMultiplier = 1.025m;
    private const decimal RosewoodBowExperiencePerLog = 420m;
    private const decimal RosewoodBonfireExperiencePerLog = 268m;
    private const decimal BowLogsPerHour = 1_485m;
    private const decimal ManualBonfireLogsPerHour = 975m;

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
                    "Use the manual Forester's Campfire rate instead of bow burning.")
            ]),
            (method, values) => CreateMethod(ResolveSettings(values)));

    public static FiremakingSettings ResolveSettings(
        TrainingConfigurationValues? configuration = null)
    {
        var values = configuration
                     ?? Configurator.Definition.Normalize();
        return new FiremakingSettings(
            values.GetToggle(PyromancerOutfitKey),
            values.GetToggle(BonfireKey));
    }

    public static TrainingMethodDefinition CreateMethod(FiremakingSettings settings)
    {
        var experienceMultiplier = settings.PyromancerOutfit
            ? PyromancerMultiplier
            : 1m;
        var rosewoodExperience = settings.UseBonfire
            ? RosewoodBonfireExperiencePerLog
            : RosewoodBowExperiencePerLog;
        var rosewoodRate = rosewoodExperience
                           * (settings.UseBonfire
                               ? ManualBonfireLogsPerHour
                               : BowLogsPerHour);
        var methodLabel = settings.UseBonfire
            ? "Rosewood logs - bonfire"
            : "Rosewood logs - bow burning";

        var method = new TrainingMethodDefinition(
            "main-ehp",
            "Rosewood logs",
            [
                Band(0, 73_700m, "Coloured logs"),
                Band(22_406, 138_900m, "Teak logs"),
                Band(45_529, 184_250m, "Arctic pine logs"),
                Band(61_512, 198_990m, "Maple logs"),
                Band(101_333, 400_271m, "Artefacts with firemaking"),
                Band(273_742, 522_696m, "Artefacts with firemaking"),
                Band(1_210_421, 768_800m, "Artefacts with firemaking"),
                Band(5_346_332, 864_981m, "Artefacts with firemaking"),
                Band(
                    13_034_431,
                    rosewoodRate,
                    methodLabel,
                    new TrainingEconomics(
                    [
                        Input(Items.RosewoodLogs, 1m / rosewoodExperience)
                    ]))
            ],
            "Pyromancer and bonfire behavior follows the saved Firemaking configuration. " +
            "Bonfire rates assume manual tending at 975 logs/hour.",
            UseStableDisplayName: true);

        return TrainingConfigurationTransforms.ApplyExperienceMultiplier(
            method,
            experienceMultiplier);
    }

    internal readonly record struct FiremakingSettings(
        bool PyromancerOutfit,
        bool UseBonfire);

    private static class Items
    {
        public static readonly CatalogueItem RosewoodLogs =
            new(32910, "Rosewood logs");
    }
}
