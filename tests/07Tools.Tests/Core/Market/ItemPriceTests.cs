namespace RunescapeTools.Tests.Core.Market;

[TestFixture]
[Category("Unit")]
public sealed class ItemPriceTests
{
    [Test]
    [Property("LegacyScenario", "MidPriceFallback")]
    [Description("mid price falls back to the available quote")]
    public void MidPriceFallback()
    {
        Assert.That(new ItemPrice(1, 777, null, null, null).MidPrice ?? 0, Is.EqualTo(777m).Within(0m), "high-only midpoint");
        Assert.That(new ItemPrice(2, null, 555, null, null).MidPrice ?? 0, Is.EqualTo(555m).Within(0m), "low-only midpoint");
    }
}
