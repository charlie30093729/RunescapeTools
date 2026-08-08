using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Hunter.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter;

internal static class HunterCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = MainEhp.Create();
        return new TrainingSkillDefinition(
            "Hunter",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                Herbiboar.Create(),
                RedChinchompas.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
