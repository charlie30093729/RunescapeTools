using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Defence.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Defence;

internal static class DefenceCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = BlackChinchompasAndCannonDefensive.Create();
        return new TrainingSkillDefinition(
            "Defence",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                RedChinchompasAndCannonDefensive.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
