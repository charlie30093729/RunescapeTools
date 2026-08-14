using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training;

internal sealed class TrainingSkillConfigurator : ITrainingSkillConfigurator
{
    private readonly Func<
        TrainingMethodDefinition,
        TrainingConfigurationValues,
        TrainingCalculationContext,
        TrainingMethodDefinition>? configure;
    private readonly Func<TrainingMethodDefinition, TrainingConfigurationValues, bool>? includeHours;

    public TrainingSkillConfigurator(
        TrainingConfigurationDefinition definition,
        Func<TrainingMethodDefinition, TrainingConfigurationValues, TrainingMethodDefinition>? configure = null,
        Func<TrainingMethodDefinition, TrainingConfigurationValues, bool>? includeHours = null)
    {
        Definition = definition;
        this.configure = configure is null
            ? null
            : (method, values, _) => configure(method, values);
        this.includeHours = includeHours;
    }

    public TrainingSkillConfigurator(
        TrainingConfigurationDefinition definition,
        Func<
            TrainingMethodDefinition,
            TrainingConfigurationValues,
            TrainingCalculationContext,
            TrainingMethodDefinition> configure,
        Func<TrainingMethodDefinition, TrainingConfigurationValues, bool>? includeHours = null)
    {
        Definition = definition;
        this.configure = configure;
        this.includeHours = includeHours;
    }

    public TrainingConfigurationDefinition Definition { get; }

    public TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues configuration,
        TrainingCalculationContext context) =>
        configure?.Invoke(method, configuration, context) ?? method;

    public bool IncludeHours(
        TrainingMethodDefinition method,
        TrainingConfigurationValues configuration) =>
        includeHours?.Invoke(method, configuration) ?? true;
}

internal static class TrainingConfigurationTransforms
{
    public static TrainingMethodDefinition ApplyExperienceMultiplier(
        TrainingMethodDefinition method,
        decimal multiplier,
        Func<TrainingRateBand, bool>? applies = null)
    {
        if (multiplier <= 0m)
            throw new ArgumentOutOfRangeException(nameof(multiplier));

        return method with
        {
            Bands = method.Bands
                .Select(band =>
                    applies is null || applies(band)
                        ? band with
                        {
                            ExperiencePerHour = band.ExperiencePerHour * multiplier,
                            Economics = ScaleEconomics(band.Economics, multiplier)
                        }
                        : band)
                .ToArray()
        };
    }

    private static TrainingEconomics? ScaleEconomics(
        TrainingEconomics? economics,
        decimal multiplier)
    {
        if (economics is null)
            return null;

        return economics with
        {
            Resources = economics.Resources
                .Select(resource => resource with
                {
                    QuantityPerExperience = resource.QuantityPerExperience / multiplier
                })
                .ToArray(),
            FixedGpPerExperience = economics.FixedGpPerExperience / multiplier,
            FixedGpOutputPerExperience = economics.FixedGpOutputPerExperience / multiplier
        };
    }
}
