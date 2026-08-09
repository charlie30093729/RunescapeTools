using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Firemaking.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Firemaking;

internal static class FiremakingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var settings = FiremakingGlobal.ResolveSettings();
        var defaultMethod = RosewoodLogs.Create(
            settings);
        var redwoodMethod = RedwoodLogs.Create(
            settings);
        return new TrainingSkillDefinition(
            "Firemaking",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods: [defaultMethod, redwoodMethod],
            DefaultMethodId: defaultMethod.Id,
            Configurator: FiremakingGlobal.Configurator);
    }
}
