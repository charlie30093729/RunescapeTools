namespace RunescapeTools.Tests.TestSupport.Builders;
internal static class MarketTestData
{
    public static ItemMapping Map(int id, string name) => new(id, name, string.Empty, true, null, string.Empty);

    public static ItemPrice Quote(int itemId, long value) => new(itemId, value, value, null, null);

    public static PricePoint Point(DateTimeOffset timestamp, long value) => new(timestamp, value, value, 10, 20);

    public static decimal Quantity(MoneyMakingResult result, string itemName) =>
        result.Lines.Single(line => line.Item.Name == itemName).QuantityPerHour;
}
