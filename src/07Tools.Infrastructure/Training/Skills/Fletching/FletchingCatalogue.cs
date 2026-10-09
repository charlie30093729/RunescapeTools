using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Fletching.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Fletching;

internal static class FletchingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var method = AmethystDarts.Create();
        return new TrainingSkillDefinition(
            "Fletching",
            method.Bands,
            Methods:
            [
                method,
                AdamantDarts.Create()
            ],
            DefaultMethodId: method.Id,
            Configurator: FletchingGlobal.Configurator);
    }
}
