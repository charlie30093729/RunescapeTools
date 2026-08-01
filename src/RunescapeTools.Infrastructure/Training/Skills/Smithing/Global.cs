using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing;

internal static class SmithingGlobal
{
    public const string SmithsUniformKey = "smiths-uniform";

    private static readonly TrainingRateBand[] MainRouteBands =
    [
        Band(0, 46_500m, "Quests"),
        Band(37_224, 380_000m, "Solo Blast Furnace gold", Methods.SoloBlastFurnaceGold.GoldEconomics(87_000m)),
        Band(273_742, 380_000m, "Solo Blast Furnace gold", Methods.SoloBlastFurnaceGold.GoldEconomics(72_000m)),
        Band(13_034_431, 410_000m, "Solo Blast Furnace gold", Methods.SoloBlastFurnaceGold.GoldEconomics(72_000m))
    ];

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    SmithsUniformKey,
                    "Smiths' uniform",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.FalseString,
                    "Shorten applicable anvil actions from five ticks to four; Blast Furnace gold is unchanged.",
                    ApplicableMethodIds:
                    [
                        "adamant-platebodies",
                        "rune-2h-swords"
                    ])
            ]),
            ConfigureMethod);

    public static IReadOnlyList<TrainingRateBand> CreateRoute(TrainingRateBand selectedMethodBand) =>
        MainRouteBands
            .Where(band => band.StartExperience < selectedMethodBand.StartExperience)
            .Append(selectedMethodBand)
            .OrderBy(band => band.StartExperience)
            .ToArray();

    private static TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues values) =>
        method.Id switch
        {
            "adamant-platebodies" => Methods.AdamantPlatebodies.Create(values.GetToggle(SmithsUniformKey)),
            "rune-2h-swords" => Methods.RuneTwoHandedSwords.Create(values.GetToggle(SmithsUniformKey)),
            _ => method
        };
}
