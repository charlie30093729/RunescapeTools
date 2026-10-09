using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Mining;
using RunescapeTools.Infrastructure.Training.Skills.Mining.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class MiningCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var main = MainEhp.Create();
        return new("Mining", main.Bands, Note: main.Note,
            Methods: [main, CalcifiedRocks.Create()], DefaultMethodId: main.Id,
            Configurator: MiningGlobal.Configurator);
    }
}
