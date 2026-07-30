using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Herblore.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore;

internal static class HerbloreCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var settings = HerbloreGlobal.ResolveSettings();
        var defaultMethod = SaradominBrews.Create(settings);

        return new TrainingSkillDefinition(
            "Herblore",
            defaultMethod.Bands,
            Note: HerbloreGlobal.EquipmentNote,
            Methods:
            [
                defaultMethod,
                SuperRestores.Create(settings),
                ExtendedSuperAntifires.Create(settings)
            ],
            DefaultMethodId: defaultMethod.Id,
            Configurator: HerbloreGlobal.Configurator);
    }
}
