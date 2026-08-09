using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Firemaking.Methods;

internal static class RosewoodLogs
{
    private const decimal BowExperiencePerLog = 420m;
    private const decimal BonfireExperiencePerLog = 268m;

    public static TrainingMethodDefinition Create(FiremakingGlobal.FiremakingSettings settings)
    {
        var experiencePerLog = settings.UseBonfire
            ? BonfireExperiencePerLog
            : BowExperiencePerLog;
        var label = settings.UseBonfire
            ? "Rosewood logs - bonfire"
            : "Rosewood logs - bow burning";
        var method = new TrainingMethodDefinition(
            "main-ehp",
            "Rosewood logs",
            [
                .. FiremakingGlobal.CreateBaseBands(),
                Band(
                    13_034_431,
                    experiencePerLog * FiremakingGlobal.LogsPerHour(settings),
                    label,
                    new TrainingEconomics(
                    [
                        Input(Items.RosewoodLogs, 1m / experiencePerLog)
                    ]))
            ],
            "Pyromancer and bonfire behavior follows the saved Firemaking configuration. " +
            "Bonfire rates assume low-effort automatic feeding at 665 logs/hour.",
            UseStableDisplayName: true);

        return FiremakingGlobal.ApplyPyromancer(method, settings);
    }

    private static class Items
    {
        public static readonly CatalogueItem RosewoodLogs = new(32910, "Rosewood logs");
    }
}
