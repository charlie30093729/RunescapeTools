using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Firemaking;

internal static class FiremakingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var method = FiremakingGlobal.CreateMethod(
            FiremakingGlobal.ResolveSettings());
        return new TrainingSkillDefinition(
            "Firemaking",
            method.Bands,
            Note: method.Note,
            Methods: [method],
            DefaultMethodId: method.Id,
            Configurator: FiremakingGlobal.Configurator);
    }
}
