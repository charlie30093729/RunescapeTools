using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Smithing.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing;

internal static class SmithingCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var defaultMethod = SoloBlastFurnaceGold.Create();
        return new TrainingSkillDefinition(
            "Smithing",
            defaultMethod.Bands,
            Note: "Smiths' uniform follows the saved Smithing configuration for applicable anvil methods and does not affect Blast Furnace gold.",
            Methods:
            [
                defaultMethod,
                AdamantPlatebodies.Create(false),
                RuneTwoHandedSwords.Create(false)
            ],
            DefaultMethodId: defaultMethod.Id,
            Configurator: SmithingGlobal.Configurator);
    }
}
