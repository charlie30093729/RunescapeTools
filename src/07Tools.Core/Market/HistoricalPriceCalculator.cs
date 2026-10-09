namespace RunescapeTools.Core.Market;

public static class HistoricalPriceCalculator
{
    // Use the last 120 completed six-hour buckets, anchored to a common UTC boundary.
    public static DateTimeOffset WindowEnd(DateTimeOffset now) =>
        DateTimeOffset.FromUnixTimeSeconds(now.ToUnixTimeSeconds() / 21_600 * 21_600);

    public static ItemPrice Calculate(int itemId, IEnumerable<PricePoint> history, DateTimeOffset end)
    {
        var start = end.AddDays(-30);
        var points = history.Where(p => p.Timestamp >= start && p.Timestamp < end
                && p.Timestamp.ToUnixTimeSeconds() % 21_600 == 0)
            .GroupBy(p => p.Timestamp).Select(g => g.Last()).ToArray();
        // Permit at most one day's missing buckets. Sparse trading is fine; a short
        // history must not be advertised as a full-month estimate.
        if (points.Length < 116)
            return new(itemId, null, null, null, null, PricingMode.ThirtyDayAverage);
        decimal? Average(bool high)
        {
            decimal value = 0m, volume = 0m;
            foreach (var point in points)
            {
                var price = high ? point.AverageHigh : point.AverageLow;
                var count = high ? point.HighVolume : point.LowVolume;
                if (price is not > 0 || count <= 0) continue;
                value += (decimal)price.Value * count;
                volume += count;
            }
            return volume > 0 ? value / volume : null;
        }
        return new(itemId, Average(true), Average(false), end, end, PricingMode.ThirtyDayAverage);
    }
}
