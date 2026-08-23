using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Fishing.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Fishing;

internal static class FishingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = MainEhp.Create();
        return new TrainingSkillDefinition(
            "Fishing",
            defaultMethod.Bands,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                ThreeTickBarbarianFishing.Create(),
                FiveTickBarbarianFishing.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
