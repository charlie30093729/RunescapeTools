namespace RunescapeTools.Tests.Core.Training;

[TestFixture]
[Category("Unit")]
public sealed class TrainingMoneyMakingCalculatorTests
{
    [Test]
    [Property("LegacyScenario", "TrainingMoneyMakerAllocation")]
    [Description("money-maker profit applies only to selected non-negative skill hours")]
    public void TrainingMoneyMakerAllocation()
    {
        var calculator = new TrainingMoneyMakingCalculator();
        var result = calculator.Calculate(2_400_000m, [10m, 2.5m, -4m]);
        Assert.That(result.SelectedHours, Is.EqualTo(12.5m).Within(0m), "selected money-making hours");
        Assert.That(result.NetGp, Is.EqualTo(30_000_000m).Within(0m), "selected money-making GP");

        var noMethod = calculator.Calculate(null, [12.5m]);
        Assert.That(noMethod.SelectedHours, Is.EqualTo(12.5m).Within(0m), "selected hours remain visible without a method");
        Assert.That(noMethod.NetGp, Is.EqualTo(0m).Within(0m), "no method contributes zero GP");
    }
}
