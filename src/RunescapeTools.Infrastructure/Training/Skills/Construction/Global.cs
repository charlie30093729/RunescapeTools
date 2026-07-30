using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Construction;

internal static class ConstructionGlobal
{
    public const string CarpentersOutfitKey = "carpenters-outfit";
    private const decimal FullOutfitMultiplier = 1.025m;

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
}
