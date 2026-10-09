namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Mining;

[TestFixture]
[Category("Unit")]
public sealed class GraniteTests
{
    [Test]
    [Property("LegacyScenario", "PhaseTwoMethodCatalogue")]
    public void ReviewedRatesAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var mining = TrainingBand(catalogue, "Mining", 393_485);

        Assert.That(mining.ExperiencePerHour, Is.EqualTo(106_540m).Within(0m), "Mining first granite rate");

        Assert.That(mining.Method, Is.EqualTo("3t4g granite - infernal pickaxe"), "Mining method");

        Assert.That(Resource(mining, 11920).QuantityPerExperience, Is.EqualTo(1m / 960_000m).Within(0m), "dragon pickaxes per Mining XP");

        Assert.That(Resource(mining, 11920).Direction, Is.EqualTo(TrainingFlowDirection.Input), "dragon pickaxe direction");
    }

    [Test]
    [Property("LegacyScenario", "PhaseTwoTrainingCalculations")]
    public void FullRoutePricingRetainsUnpricedEarlyLevels()
    {
        var catalogue = new MainEhpCatalogue();

        var calculator = new TrainingPlanCalculator();

        var prices = new Dictionary<int, ItemPrice>
        {
            [23959] = Quote(23959, 3_000_000),
            [28157] = Quote(28157, 50),
            [11920] = Quote(11920, 1_000_000),
            [11959] = Quote(11959, 3_000),
            [7936] = Quote(7936, 1),
            [557] = Quote(557, 5),
            [5521] = Quote(5521, 1_000),
            [9075] = Quote(9075, 200),
            [556] = Quote(556, 5),
            [564] = Quote(564, 100),
            [4698] = Quote(4698, 100)
        };

        var mining = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Mining"),
            0,
            TrainingPlanCalculator.MaximumExperience,
            prices);

        Assert.That(mining.PricedExperience, Is.EqualTo(199_606_515L), "priced Mining XP");

        Assert.That(mining.NetGp ?? 0m, Is.EqualTo(-(199_606_515m / 960_000m * 1_000_000m)).Within(0.01m), "Mining infernal-pickaxe cost");

        Assert.That(!mining.IsFullyPriced, Is.True, "early Mining should remain visibly unpriced");
    }
}
