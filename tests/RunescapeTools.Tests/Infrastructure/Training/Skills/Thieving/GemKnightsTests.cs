namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Thieving;

[TestFixture]
[Category("Unit")]
public sealed class GemKnightsTests
{
    [Test]
    [Property("LegacyScenario", "PhaseThreeMethodCatalogue")]
    public void ReviewedRatesAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var thieving = TrainingBand(catalogue, "Thieving", 0);

        Assert.That(thieving.ExperiencePerHour, Is.EqualTo(260_000m).Within(0m), "Gem knights rate");

        Assert.That(thieving.Method, Is.EqualTo("Gem knights"), "Thieving method");

        Assert.That(Resource(thieving, 6571).QuantityPerExperience, Is.EqualTo((182m / 195m) * 5m * 1.8m / 260_000m / 103.4m).Within(0m), "Gem knights Tokkul-to-onyx output per XP");

        Assert.That(thieving.Economics?.Resources.Count ?? 0, Is.EqualTo(1), "Gem knights price only Tokkul conversion");

        Assert.That(thieving.Economics is { IsComplete: true }, Is.True, "Gem knights Tokkul projection should be priced");
    }

    [Test]
    [Property("LegacyScenario", "PhaseThreeTrainingCalculations")]
    public void FullRouteHoursAndTokkulEconomics()
    {
        var catalogue = new MainEhpCatalogue();

        var calculator = new TrainingPlanCalculator();

        var thieving = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Thieving"),
            0,
            TrainingPlanCalculator.MaximumExperience,
            new Dictionary<int, ItemPrice> { [6571] = Quote(6571, 2_600_000) });

        Assert.That(thieving.Hours, Is.EqualTo(769.2308m).Within(0.0001m), "Gem knights 0-200m hours");

        var expectedTokkul = TrainingPlanCalculator.MaximumExperience
                             / 103.4m
                             * (182m / 195m)
                             * 5m
                             * 1.8m;

        var expectedOnyx = expectedTokkul / 260_000m;

        Assert.That(thieving.NetGp ?? 0m, Is.EqualTo(expectedOnyx * 2_548_000m).Within(0.01m), "Gem knights Tokkul-to-onyx value");

        Assert.That(thieving.IsFullyPriced, Is.True, "Gem knights Tokkul projection should be fully priced");
    }
}
