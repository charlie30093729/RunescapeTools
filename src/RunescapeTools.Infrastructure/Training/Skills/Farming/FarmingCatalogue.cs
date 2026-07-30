using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Farming.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Farming;

internal static class FarmingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = MagicDragonfruitTreeRuns.Create();

        return new TrainingSkillDefinition(
            "Farming",
            defaultMethod.Bands,
            Note: FarmingGlobal.Note,
            Methods:
            [
                defaultMethod,
                MagicPalmTreeRuns.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
