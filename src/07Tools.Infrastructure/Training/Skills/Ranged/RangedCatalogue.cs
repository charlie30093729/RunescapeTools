using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Ranged.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Ranged;

internal static class RangedCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = BlackChinchompasAndCannon.Create();
        return new TrainingSkillDefinition(
            "Ranged",
            defaultMethod.Bands,
            IsZeroTime: true,
            Note: defaultMethod.Note,
            Methods:
            [
                defaultMethod,
                RedChinchompasAndCannon.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
