using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Hunter.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter;

internal static class HunterCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = BlackChinchompas.Create();
        return new TrainingSkillDefinition(
            "Hunter",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                Herbiboar.Create(),
                RedChinchompas.Create(),
                AerialFishing.Create()
            ],
            DefaultMethodId: defaultMethod.Id,
            Configurator: HunterGlobal.Configurator);
    }
}
