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
    decimal QuantityPerHour = 0m);

public sealed record TrainingEconomics(
    IReadOnlyList<TrainingResourceFlow> Resources,
    decimal FixedGpPerExperience = 0m,
    bool IsComplete = true,
    decimal FixedGpPerHour = 0m);

public sealed record TrainingRateBand(
    long StartExperience,
    decimal ExperiencePerHour,
    string Method,
    TrainingEconomics? Economics = null);

public sealed record TrainingSkillDefinition(
    string Skill,
    IReadOnlyList<TrainingRateBand> Bands,
    bool IsZeroTime = false,
    string? Note = null);

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

public sealed record TrainingSkillPlanResult(
    TrainingSkillDefinition Definition,
    long StartExperience,
    long TargetExperience,
    decimal BaseRate,
    decimal EffectiveRate,
    decimal Hours,
    decimal? NetGp,
    long PricedExperience,
    bool UsedFallbackPrice,
    bool HasMissingPrice,
    IReadOnlyList<TrainingBandResult> Bands)
{
    public long ExperienceRemaining => Math.Max(0, TargetExperience - StartExperience);
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
        decimal? personalRate = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(prices);

        var start = Math.Clamp(startExperience, 0, MaximumExperience);
        var target = Math.Clamp(targetExperience, start, MaximumExperience);
        var ordered = definition.Bands.OrderBy(band => band.StartExperience).ToArray();
        var activeBand = ordered.LastOrDefault(band => band.StartExperience <= start)
                         ?? ordered.FirstOrDefault();
        var baseRate = activeBand?.ExperiencePerHour ?? 0m;
        var multiplier = personalRate is > 0m && baseRate > 0m
            ? personalRate.Value / baseRate
            : 1m;

        if (definition.IsZeroTime || target == start || ordered.Length == 0)
        {
            return new TrainingSkillPlanResult(
                definition, start, target, baseRate, personalRate ?? baseRate, 0m, 0m,
                target - start, false, false, []);
        }

        var results = new List<TrainingBandResult>();
        decimal totalHours = 0m;
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
            var segmentStart = Math.Max(start, band.StartExperience);
            var segmentEnd = Math.Min(target, nextStart);
            if (segmentEnd <= segmentStart)
                continue;

            var experience = segmentEnd - segmentStart;
            var effectiveBandRate = band.ExperiencePerHour * multiplier;
            var hours = effectiveBandRate > 0m ? experience / effectiveBandRate : 0m;
            totalHours += hours;

            decimal? segmentGp = null;
            var segmentFallback = false;
            var segmentMissing = false;
            if (band.Economics is { IsComplete: true } economics)
            {
                var gpPerExperience = -economics.FixedGpPerExperience;
                if (economics.FixedGpPerHour != 0m && effectiveBandRate > 0m)
                    gpPerExperience -= economics.FixedGpPerHour / effectiveBandRate;

                foreach (var resource in economics.Resources)
                {
                    if (!prices.TryGetValue(resource.ItemId, out var quote))
                    {
                        segmentMissing = true;
                        break;
                    }

                    var unitPrice = SelectPrice(resource.Direction, quote, out var fallback);
                    segmentFallback |= fallback;
                    if (!unitPrice.HasValue)
                    {
                        segmentMissing = true;
                        break;
                    }

                    var quantityPerExperience = resource.QuantityPerExperience;
                    if (resource.QuantityPerHour != 0m && effectiveBandRate > 0m)
                        quantityPerExperience += resource.QuantityPerHour / effectiveBandRate;

                    var value = unitPrice.Value * quantityPerExperience;
                    if (resource.Direction == TrainingFlowDirection.Input)
                    {
                        gpPerExperience -= value;
                    }
                    else
                    {
                        if (resource.SubjectToGeTax)
                        {
                            var taxPerItem = Math.Min(Math.Floor(unitPrice.Value * GeTaxRate), GeTaxCapPerItem);
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
                band, segmentStart, segmentEnd, hours, segmentGp, segmentFallback, segmentMissing));
        }

        return new TrainingSkillPlanResult(
            definition,
            start,
            target,
            baseRate,
            personalRate ?? baseRate,
            totalHours,
            hasAnyPrice ? totalGp : null,
            pricedExperience,
            usedFallback,
            hasMissing,
            results);
    }

    private static decimal? SelectPrice(
        TrainingFlowDirection direction,
        ItemPrice quote,
        out bool usedFallback)
    {
        if (direction == TrainingFlowDirection.Input)
        {
            usedFallback = quote.High is null && quote.Low is not null;
            return quote.High ?? quote.Low;
        }

        usedFallback = quote.Low is null && quote.High is not null;
        return quote.Low ?? quote.High;
    }
}
