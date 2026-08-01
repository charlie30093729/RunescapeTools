using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Construction.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction;

internal static class ConstructionCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = MainEhp.Create();
        return new TrainingSkillDefinition(
            "Construction",
            defaultMethod.Bands,
            Note: "Carpenter's outfit follows the saved Construction configuration.",
            Methods:
            [
                defaultMethod,
                OakDungeonDoors.Create()
            ],
            DefaultMethodId: defaultMethod.Id,
            Configurator: ConstructionGlobal.Configurator);
    }
}
