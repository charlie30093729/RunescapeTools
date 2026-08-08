using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Cooking.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class CookingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = MainEhp.Create();
        return new TrainingSkillDefinition(
            "Cooking",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                OneTickKarambwans.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
