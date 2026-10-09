using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction;

internal static class ConstructionGlobal
{
    public const string CarpentersOutfitKey = "carpenters-outfit";
    private const decimal FullOutfitMultiplier = 1.025m;
    private const decimal DemonButlerGpPerTrip = 10_000m / 8m;

    private static readonly TrainingRateBand[] MainRouteBands =
    [
        Band(0, 54_700m, "Low-level furniture"),
        Band(18_247, 200_000m, "Oak larders", PlankEconomics(Methods.MainEhp.Items.OakPlank, 60m, 24m)),
        Band(37_224, 290_000m, "Mahogany bookcases", PlankEconomics(Methods.MainEhp.Items.MahoganyPlank, 140m, 24m)),
        Band(123_660, 950_000m, "Mahogany tables", PlankEconomics(Methods.MainEhp.Items.MahoganyPlank, 140m, 24m)),
        Band(1_475_581, 1_070_000m, "Mahogany benches", PlankEconomics(Methods.MainEhp.Items.MahoganyPlank, 140m, 24m)),
        Band(13_034_431, 1_440_000m, "2t mahogany flatpacks", PlankEconomics(Methods.MainEhp.Items.MahoganyPlank, 140m, 24m))
    ];

    public static ITrainingSkillConfigurator Configurator { get; } =
        new TrainingSkillConfigurator(
            new TrainingConfigurationDefinition(
            [
                new TrainingConfigurationOption(
                    CarpentersOutfitKey,
                    "Carpenter's outfit",
                    TrainingConfigurationOptionKind.Toggle,
                    bool.FalseString,
                    "Apply the full outfit's 2.5% Construction XP bonus.")
            ]),
            (method, values) =>
                values.GetToggle(CarpentersOutfitKey)
                    ? TrainingConfigurationTransforms.ApplyExperienceMultiplier(
                        method,
                        FullOutfitMultiplier)
                    : method);

    public static IReadOnlyList<TrainingRateBand> CreateRoute(TrainingRateBand selectedMethodBand) =>
        MainRouteBands
            .Where(band => band.StartExperience < selectedMethodBand.StartExperience)
            .Append(selectedMethodBand)
            .OrderBy(band => band.StartExperience)
            .ToArray();

    public static TrainingEconomics PlankEconomics(
        CatalogueItem plank,
        decimal experiencePerPlank,
        decimal servantCapacity) =>
        new(
            [Input(plank, 1m / experiencePerPlank)],
            DemonButlerGpPerTrip / servantCapacity / experiencePerPlank);
}
