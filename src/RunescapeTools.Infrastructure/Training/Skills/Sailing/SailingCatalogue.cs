using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Sailing;
using RunescapeTools.Infrastructure.Training.Skills.Sailing.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class SailingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = GwenithGlide.Create();
        return new("Sailing", defaultMethod.Bands, Note: defaultMethod.Note,
            Methods: [defaultMethod, Salvaging.Create()], DefaultMethodId: defaultMethod.Id,
            Configurator: SailingGlobal.Configurator);
    }
}
