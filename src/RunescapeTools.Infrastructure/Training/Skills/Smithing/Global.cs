using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing;

internal static class SmithingGlobal
{
    public const string SmithsUniformKey = "smiths-uniform";

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    SmithsUniformKey,
                    "Smiths' uniform",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.FalseString,
                    "Saved now for future rune and adamant anvil methods. It has no effect on Blast Furnace gold.")
            ]));
}
