using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Fletching;

internal static class FletchingGlobal
{
    public const string IncludeHoursKey = "include-hours";

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    IncludeHoursKey,
                    "Include active hours",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.TrueString,
                    "Turn off when Fletching is completed during other activities.")
            ]),
            includeHours: (_, values) => values.GetToggle(IncludeHoursKey));
}
