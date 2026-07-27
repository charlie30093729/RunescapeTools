using RunescapeTools.Core.Market;

namespace RunescapeTools.Core.Training;

public sealed record TrainingMarketPrice(
    long? UnitPrice,
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
