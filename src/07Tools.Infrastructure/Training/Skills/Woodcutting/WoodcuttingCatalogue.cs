using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Woodcutting.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Woodcutting;

internal static class WoodcuttingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = MainEhp.Create();
        return new TrainingSkillDefinition(
            "Woodcutting",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                RedwoodTrees.Create(),
                IronwoodTrees.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
