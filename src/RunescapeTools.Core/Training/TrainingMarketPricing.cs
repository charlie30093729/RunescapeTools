using RunescapeTools.Core.Market;

namespace RunescapeTools.Core.Training;

public sealed record TrainingMarketPrice(
    decimal? UnitPrice,
    DateTimeOffset? Timestamp,
    bool UsedFallbackPrice);

public static class TrainingMarketPricing
{
    public static TrainingMarketPrice Select(
        TrainingFlowDirection direction,
        ItemPrice? quote)
    {
        if (quote is null)
            return new TrainingMarketPrice(null, null, false);

        // Historical buy and sell averages are separate populations; never substitute
        // the opposite side when the requested side has insufficient observations.
        if (quote.Basis == PricingMode.ThirtyDayAverage)
            return direction == TrainingFlowDirection.Input
                ? new(quote.High, quote.HighTime, false)
                : new(quote.Low, quote.LowTime, false);

        if (direction == TrainingFlowDirection.Input)
        {
            return quote.High.HasValue
                ? new TrainingMarketPrice(quote.High, quote.HighTime, false)
                : new TrainingMarketPrice(quote.Low, quote.LowTime, quote.Low.HasValue);
        }

        return quote.Low.HasValue
            ? new TrainingMarketPrice(quote.Low, quote.LowTime, false)
            : new TrainingMarketPrice(quote.High, quote.HighTime, quote.High.HasValue);
    }
}
