using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Fletching;

internal static class FletchingGlobal
{
    public const string IncludeHoursKey = "include-hours";

    private static readonly TrainingRateBand[] BaseRouteBands =
    [
        Band(0, 1_000_000m, "Zero-time Fletching - rate only")
    ];

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

    public static IReadOnlyList<TrainingRateBand> CreateRoute(TrainingRateBand selectedMethodBand) =>
        BaseRouteBands
            .Where(band => band.StartExperience < selectedMethodBand.StartExperience)
            .Append(selectedMethodBand)
            .OrderBy(band => band.StartExperience)
            .ToArray();
}
