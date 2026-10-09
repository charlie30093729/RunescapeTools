namespace RunescapeTools.Tests.Core.MoneyMaking;

[TestFixture]
[Category("Unit")]
public sealed class MoneyMakingCalculatorTests
{
    [Test]
    [Property("LegacyScenario", "GenericFlowCalculation")]
    [Description("generic flow calculation")]
    public void GenericFlowCalculation()
    {
        var method = new MoneyMakingMethodDefinition(
            "test", "Test", "Known values", 10m, 2, 0.02m,
            [
                new ItemFlow(1, "Input", 3m, ItemFlowDirection.Input),
                new ItemFlow(2, "Output", 0.5m, ItemFlowDirection.Output, QuantityBasis.PerAction)
            ]);
        var prices = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100), [2] = Quote(2, 1000) };

        var result = new MoneyMakingCalculator().Calculate(method, prices);

        Assert.That(result.GrossRevenuePerAccount, Is.EqualTo(5_000m).Within(0m), "gross revenue");
        Assert.That(result.TaxPerAccount, Is.EqualTo(100m).Within(0m), "tax");
        Assert.That(result.InputCostPerAccount, Is.EqualTo(300m).Within(0m), "input cost");
        Assert.That(result.ProfitPerAccount, Is.EqualTo(4_600m).Within(0m), "profit per account");
        Assert.That(result.ProfitAllAccounts, Is.EqualTo(9_200m).Within(0m), "profit for all accounts");

        var fourAccounts = new MoneyMakingCalculator().Calculate(method, prices, 4);
        Assert.That(fourAccounts.Method.Accounts, Is.EqualTo(4), "manual account quantity");
        Assert.That(fourAccounts.ProfitAllAccounts, Is.EqualTo(18_400m).Within(0m), "manual account total profit");
    }
}
