using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Herblore.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Herblore;

internal static class HerbloreCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = SaradominBrews.Create();

        return new TrainingSkillDefinition(
            "Herblore",
            defaultMethod.Bands,
            Note: HerbloreGlobal.EquipmentNote,
            Methods:
            [
                defaultMethod,
                SuperRestores.Create(),
                ExtendedSuperAntifires.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
