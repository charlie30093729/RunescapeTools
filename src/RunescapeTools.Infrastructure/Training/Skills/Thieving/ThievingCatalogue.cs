using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Thieving.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class ThievingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var main = GemKnights.Create();
        return new("Thieving", main.Bands, Note: main.Note,
            Methods: [main, ArdougneKnights.Create()],
            DefaultMethodId: main.Id);
    }
}
