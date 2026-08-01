using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Crafting.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Crafting;

internal static class CraftingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = BlackDragonhideBodies.Create();
        return new TrainingSkillDefinition(
            "Crafting",
            defaultMethod.Bands,
            Methods:
            [
                defaultMethod,
                AirBattlestaves.Create()
            ],
            DefaultMethodId: defaultMethod.Id);
    }
}
