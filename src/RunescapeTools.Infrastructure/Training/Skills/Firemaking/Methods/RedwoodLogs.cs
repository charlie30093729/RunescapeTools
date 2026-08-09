using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Firemaking.Methods;

internal static class RedwoodLogs
{
    private const long UnlockExperience = 5_346_332;
    private const decimal ExperiencePerLog = 350m;

    public static TrainingMethodDefinition Create(FiremakingGlobal.FiremakingSettings settings)
    {
        var label = settings.UseBonfire
            ? "Redwood logs - bonfire"
            : "Redwood logs - normal burning";
        var method = new TrainingMethodDefinition(
            "redwood-logs",
            "Redwood logs",
            [
                .. FiremakingGlobal.CreateBaseBands()
                    .Where(band => band.StartExperience < UnlockExperience),
                Band(
                    UnlockExperience,
                    ExperiencePerLog * FiremakingGlobal.LogsPerHour(settings),
                    label,
                    new TrainingEconomics(
                    [
                        Input(Items.RedwoodLogs, 1m / ExperiencePerLog)
                    ]))
            ],
            "Requires level 90 Firemaking. Pyromancer and bonfire behavior follows the saved " +
            "Firemaking configuration. Normal burning assumes 1,485 logs/hour; bonfires assume " +
            "low-effort automatic feeding at 665 logs/hour.",
            UseStableDisplayName: true);

        return FiremakingGlobal.ApplyPyromancer(method, settings);
    }

    private static class Items
    {
        public static readonly CatalogueItem RedwoodLogs = new(19669, "Redwood logs");
    }
}
