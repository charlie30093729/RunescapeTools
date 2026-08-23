using RunescapeTools.Core.Market;

namespace RunescapeTools.Core.Training;

public enum TrainingFlowDirection
{
    Input,
    Output
}

public sealed record TrainingResourceFlow(
    int ItemId,
    string Name,
    decimal QuantityPerExperience,
    TrainingFlowDirection Direction,
    bool SubjectToGeTax = true,
    decimal QuantityPerHour = 0m,
    bool RequiresMarketPrice = true);

public sealed record TrainingExperienceFlow(
    string Skill,
    decimal QuantityPerPrimaryExperience,
    decimal QuantityPerHour = 0m);

public sealed record TrainingEconomics(
    IReadOnlyList<TrainingResourceFlow> Resources,
    decimal FixedGpPerExperience = 0m,
    bool IsComplete = true,
    decimal FixedGpPerHour = 0m,
    decimal FixedGpOutputPerExperience = 0m,
    decimal FixedGpOutputPerHour = 0m);

public sealed record TrainingRateBand(
    long StartExperience,
    decimal ExperiencePerHour,
    string Method,
    TrainingEconomics? Economics = null,
    decimal ConfigurationRateMultiplier = 1m,
    IReadOnlyList<TrainingExperienceFlow>? ExperienceOutputs = null);

public sealed record TrainingMethodDefinition(
    string Id,
    string Name,
    IReadOnlyList<TrainingRateBand> Bands,
    string? Note = null,
    IReadOnlyList<TrainingExperienceFlow>? ExperienceOutputs = null,
    bool UseStableDisplayName = false);

public sealed record TrainingSkillDefinition(
    string Skill,
    IReadOnlyList<TrainingRateBand> Bands,
    bool IsZeroTime = false,
    string? Note = null,
    IReadOnlyList<TrainingMethodDefinition>? Methods = null,
    string DefaultMethodId = "main-ehp",
    IReadOnlyList<TrainingExperienceFlow>? ExperienceOutputs = null,
    ITrainingSkillConfigurator? Configurator = null)
{
    public IReadOnlyList<TrainingMethodDefinition> AvailableMethods =>
        Methods is { Count: > 0 }
            ? Methods
            : [new TrainingMethodDefinition(DefaultMethodId, "Main EHP route", Bands, Note, ExperienceOutputs)];

    public TrainingMethodDefinition ResolveMethod(string? methodId = null)
    {
        var resolvedId = string.IsNullOrWhiteSpace(methodId) ? DefaultMethodId : methodId;
        return AvailableMethods.FirstOrDefault(
                   method => string.Equals(method.Id, resolvedId, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException(
                   $"Training method '{resolvedId}' is not registered for {Skill}.",
                   nameof(methodId));
    }
}

public sealed record TrainingBandResult(
    TrainingRateBand Band,
    long StartExperience,
    long EndExperience,
    decimal Hours,
    decimal? NetGp,
    bool UsedFallbackPrice,
    bool HasMissingPrice)
{
    public long Experience => EndExperience - StartExperience;
}

public sealed record TrainingResourceRequirement(
    int ItemId,
    string Name,
    TrainingFlowDirection Direction,
    decimal Quantity,
    bool SubjectToGeTax,
    bool RequiresMarketPrice);

public sealed record TrainingSkillPlanResult(
    TrainingSkillDefinition Definition,
    TrainingMethodDefinition Method,
    long StartExperience,
    long TargetExperience,
    decimal BaseRate,
    decimal EffectiveRate,
    decimal Hours,
    decimal? NetGp,
    long PricedExperience,
    bool UsedFallbackPrice,
    bool HasMissingPrice,
    IReadOnlyList<TrainingBandResult> Bands,
    IReadOnlyList<TrainingResourceRequirement> ResourceRequirements,
    long AppliedExperienceCredit,
    IReadOnlyDictionary<string, decimal> GeneratedExperience,
    bool IncludesActiveHours)
{
    public long EffectiveStartExperience =>
        Math.Min(TargetExperience, StartExperience + AppliedExperienceCredit);
    public long RawExperienceRemaining => Math.Max(0, TargetExperience - StartExperience);
    public long ExperienceRemaining => Math.Max(0, TargetExperience - EffectiveStartExperience);
    public bool IsFullyPriced => ExperienceRemaining == 0 || PricedExperience >= ExperienceRemaining;
    public decimal? GpPerExperience => NetGp.HasValue && ExperienceRemaining > 0
        ? NetGp.Value / ExperienceRemaining
        : null;
    public decimal? AverageGpPerHour => NetGp.HasValue && Hours > 0
        ? NetGp.Value / Hours
        : null;
}

public sealed class TrainingPlanCalculator
{
    public const long MaximumExperience = 200_000_000;
    private const decimal GeTaxRate = 0.02m;
    private const decimal GeTaxCapPerItem = 5_000_000m;

    public TrainingSkillPlanResult Calculate(
        TrainingSkillDefinition definition,
        long startExperience,
        long targetExperience,
        IReadOnlyDictionary<int, ItemPrice> prices,
        decimal? personalRate = null,
        string? methodId = null,
        long pendingExperienceCredit = 0,
        IReadOnlyDictionary<string, string>? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(prices);

        var start = Math.Clamp(startExperience, 0, MaximumExperience);
        var target = Math.Clamp(targetExperience, start, MaximumExperience);
        var appliedCredit = Math.Clamp(pendingExperienceCredit, 0, target - start);
        var effectiveStart = start + appliedCredit;
        var baseMethod = definition.ResolveMethod(methodId);
        var configurationValues =
            definition.Configurator?.Definition.Normalize(configuration)
            ?? TrainingConfigurationValues.Empty;
        var method = definition.Configurator?.ConfigureMethod(
                         baseMethod,
                         configurationValues,
                         new TrainingCalculationContext(effectiveStart, target))
                     ?? baseMethod;
        var includesActiveHours =
            !definition.IsZeroTime
            && (definition.Configurator?.IncludeHours(method, configurationValues) ?? true);
        var ordered = method.Bands.OrderBy(band => band.StartExperience).ToArray();
        var activeBand = ordered.LastOrDefault(band => band.StartExperience <= effectiveStart)
                         ?? ordered.FirstOrDefault();
        var baseRate = activeBand?.ExperiencePerHour ?? 0m;
        var activeConfigurationMultiplier = activeBand?.ConfigurationRateMultiplier is > 0m
            ? activeBand.ConfigurationRateMultiplier
            : 1m;
        var unconfiguredBaseRate = baseRate / activeConfigurationMultiplier;
        var personalRateMultiplier = personalRate is > 0m && unconfiguredBaseRate > 0m
            ? personalRate.Value / unconfiguredBaseRate
            : 1m;
        var effectiveRate = personalRate is > 0m
            ? personalRate.Value * activeConfigurationMultiplier
            : baseRate;

        if (target == effectiveStart || ordered.Length == 0)
        {
            return new TrainingSkillPlanResult(
                definition, method, start, target, baseRate, effectiveRate, 0m, 0m,
                target - effectiveStart, false, false, [], [], appliedCredit,
                new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase),
                includesActiveHours);
        }

        var results = new List<TrainingBandResult>();
        var resourceRequirements = new Dictionary<
            (int ItemId, TrainingFlowDirection Direction),
            (TrainingResourceFlow Resource, decimal Quantity)>();
        var generatedExperience = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        decimal calculationHours = 0m;
        decimal totalGp = 0m;
        long pricedExperience = 0;
        var hasAnyPrice = false;
        var usedFallback = false;
        var hasMissing = false;

        for (var index = 0; index < ordered.Length; index++)
        {
            var band = ordered[index];
            var nextStart = index + 1 < ordered.Length
                ? ordered[index + 1].StartExperience
                : MaximumExperience;
            var segmentStart = Math.Max(effectiveStart, band.StartExperience);
            var segmentEnd = Math.Min(target, nextStart);
            if (segmentEnd <= segmentStart)
                continue;

            var experience = segmentEnd - segmentStart;
            var effectiveBandRate = band.ExperiencePerHour * personalRateMultiplier;
            var hours = effectiveBandRate > 0m ? experience / effectiveBandRate : 0m;
            calculationHours += hours;

            foreach (var flow in band.ExperienceOutputs ?? [])
            {
                var quantity = flow.QuantityPerPrimaryExperience * experience
                               + flow.QuantityPerHour * hours;
                generatedExperience[flow.Skill] =
                    generatedExperience.GetValueOrDefault(flow.Skill) + quantity;
            }

            foreach (var resource in band.Economics?.Resources ?? [])
            {
                var quantity = resource.QuantityPerExperience * experience
                               + resource.QuantityPerHour * hours;
                var key = (resource.ItemId, resource.Direction);
                if (resourceRequirements.TryGetValue(key, out var existing))
                    resourceRequirements[key] = (existing.Resource, existing.Quantity + quantity);
                else
                    resourceRequirements[key] = (resource, quantity);
            }

            decimal? segmentGp = null;
            var segmentFallback = false;
            var segmentMissing = false;
            if (band.Economics is { IsComplete: true } economics)
            {
                var gpPerExperience =
                    economics.FixedGpOutputPerExperience - economics.FixedGpPerExperience;
                if (effectiveBandRate > 0m)
                {
                    gpPerExperience +=
                        (economics.FixedGpOutputPerHour - economics.FixedGpPerHour)
                        / effectiveBandRate;
                }

                foreach (var resource in economics.Resources)
                {
                    if (!resource.RequiresMarketPrice)
                        continue;

                    if (!prices.TryGetValue(resource.ItemId, out var quote))
                    {
                        segmentMissing = true;
                        break;
                    }

                    var marketPrice = TrainingMarketPricing.Select(resource.Direction, quote);
                    segmentFallback |= marketPrice.UsedFallbackPrice;
                    if (!marketPrice.UnitPrice.HasValue)
                    {
                        segmentMissing = true;
                        break;
                    }

                    var quantityPerExperience = resource.QuantityPerExperience;
                    if (resource.QuantityPerHour != 0m && effectiveBandRate > 0m)
                        quantityPerExperience += resource.QuantityPerHour / effectiveBandRate;

                    var value = marketPrice.UnitPrice.Value * quantityPerExperience;
                    if (resource.Direction == TrainingFlowDirection.Input)
                    {
                        gpPerExperience -= value;
                    }
                    else
                    {
                        if (resource.SubjectToGeTax)
                        {
                            var taxPerItem = Math.Min(
                                Math.Floor(marketPrice.UnitPrice.Value * GeTaxRate),
                                GeTaxCapPerItem);
                            value -= taxPerItem * quantityPerExperience;
                        }

                        gpPerExperience += value;
                    }
                }

                if (!segmentMissing)
                {
                    segmentGp = gpPerExperience * experience;
                    totalGp += segmentGp.Value;
                    pricedExperience += experience;
                    hasAnyPrice = true;
                }
            }

            usedFallback |= segmentFallback;
            hasMissing |= segmentMissing;
            results.Add(new TrainingBandResult(
                band,
                segmentStart,
                segmentEnd,
                includesActiveHours ? hours : 0m,
                segmentGp,
                segmentFallback,
                segmentMissing));
        }

        foreach (var flow in method.ExperienceOutputs ?? [])
        {
            var quantity = flow.QuantityPerPrimaryExperience * (target - effectiveStart)
                           + flow.QuantityPerHour * calculationHours;
            generatedExperience[flow.Skill] =
                generatedExperience.GetValueOrDefault(flow.Skill) + quantity;
        }

        return new TrainingSkillPlanResult(
            definition,
            method,
            start,
            target,
            baseRate,
            effectiveRate,
            includesActiveHours ? calculationHours : 0m,
            hasAnyPrice ? totalGp : null,
            pricedExperience,
            usedFallback,
            hasMissing,
            results,
            resourceRequirements.Values
                .Where(value => value.Quantity != 0m)
                .Select(value => new TrainingResourceRequirement(
                    value.Resource.ItemId,
                    value.Resource.Name,
                    value.Resource.Direction,
                    value.Quantity,
                    value.Resource.SubjectToGeTax,
                    value.Resource.RequiresMarketPrice))
                .OrderBy(value => value.Direction)
                .ThenBy(value => value.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            appliedCredit,
            generatedExperience,
            includesActiveHours);
    }

}
