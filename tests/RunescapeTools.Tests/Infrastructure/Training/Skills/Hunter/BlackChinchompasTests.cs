namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Hunter;

[TestFixture]
[Category("Unit")]
public sealed class BlackChinchompasTests
{
    [Test]
    [Property("LegacyScenario", "PhaseTwoMethodCatalogue")]
    public void ReviewedRatesAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var hunter = TrainingBand(catalogue, "Hunter", 992_895);

        Assert.That(hunter.ExperiencePerHour, Is.EqualTo(265_000m).Within(0m), "Hunter rate");

        Assert.That(hunter.Method, Is.EqualTo("Black chinchompas - shooting alt"), "Hunter method");

        Assert.That(Resource(hunter, 11959).QuantityPerExperience, Is.EqualTo(1m / 315m).Within(0m), "black chins per Hunter XP");

        Assert.That(Resource(hunter, 11959).Direction, Is.EqualTo(TrainingFlowDirection.Output), "black chin direction");

        Assert.That(Resource(hunter, 11959).SubjectToGeTax, Is.True, "black chins should be GE taxed");
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

        var hunter = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Hunter"),
            0,
            TrainingPlanCalculator.MaximumExperience,
            prices);

        Assert.That(hunter.PricedExperience, Is.EqualTo(199_007_105L), "priced Hunter XP");

        Assert.That(hunter.NetGp ?? 0m, Is.EqualTo(199_007_105m / 315m * 2_940m).Within(0.01m), "Hunter black-chin revenue after tax");

        Assert.That(hunter.NetGp > 0m, Is.True, "black chinchompas should produce profit");
    }
}
