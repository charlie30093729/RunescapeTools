namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Thieving;

[TestFixture]
[Category("Unit")]
public sealed class ArdougneKnightsTests
{
    private static TrainingSkillDefinition Definition() =>
        new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Thieving");

    [TestCase(166636, 82369)]
    [TestCase(273742, 93894)]
    [TestCase(449428, 105889)]
    [TestCase(737627, 121154)]
    [TestCase(1210421, 139152)]
    [TestCase(1986068, 158557)]
    [TestCase(3258594, 184303)]
    [TestCase(5346332, 216281)]
    [TestCase(8771558, 252900)]
    public void ReviewedLevelBandsPriceCoinsWithoutMarketQuotes(long start, int rate)
    {
        var result = new TrainingPlanCalculator().Calculate(Definition(), start, start + rate,
            new Dictionary<int, ItemPrice>(), methodId: "ardougne-knights");

        Assert.That(result.BaseRate, Is.EqualTo(rate));
        Assert.That(result.Hours, Is.EqualTo(1m));
        Assert.That(result.NetGp, Is.EqualTo(rate * 100m / 84.3m).Within(0.000001m));
        Assert.That(result.IsFullyPriced, Is.True);
        Assert.That(result.HasMissingPrice, Is.False);
        Assert.That(result.ResourceRequirements, Is.Empty);
        Assert.That(result.GeneratedExperience, Is.Empty);
    }

    [Test]
    public void RealUnlockRetainsGemKnightFallbackAndSplitsCoinEconomics()
    {
        var definition = Definition();
        var calculator = new TrainingPlanCalculator();
        var prices = new Dictionary<int, ItemPrice> { [6571] = Quote(6571, 2_600_000) };
        var fallback = calculator.Calculate(definition, 166_536, 166_636, prices);
        var result = calculator.Calculate(definition, 166_536, 166_736, prices,
            methodId: "ardougne-knights");

        Assert.That(result.Bands, Has.Count.EqualTo(2));
        Assert.That(result.Bands[0].Band.Method, Is.EqualTo(definition.Bands[0].Method));
        Assert.That(result.Bands[0].Band.ExperiencePerHour,
            Is.EqualTo(definition.Bands[0].ExperiencePerHour));
        Assert.That(result.Bands[0].Band.Economics!.Resources,
            Is.EqualTo(definition.Bands[0].Economics!.Resources));
        Assert.That(result.Bands[1].Band.Method, Is.EqualTo("Ardougne knights"));
        Assert.That(result.Hours, Is.EqualTo(100m / 260_000m + 100m / 82_369m));
        Assert.That(result.NetGp, Is.EqualTo(fallback.NetGp!.Value + 100m * 100m / 84.3m)
            .Within(0.000001m));
        Assert.That(result.ResourceRequirements, Is.EqualTo(fallback.ResourceRequirements));
    }

    [Test]
    public void CrossingRateBoundaryUsesBothRatesAndKeepsCoinYieldPerExperience()
    {
        var result = new TrainingPlanCalculator().Calculate(Definition(), 8_770_558, 8_772_558,
            new Dictionary<int, ItemPrice>(), methodId: "ardougne-knights");

        Assert.That(result.Bands, Has.Count.EqualTo(2));
        Assert.That(result.Hours, Is.EqualTo(1_000m / 216_281m + 1_000m / 252_900m));
        Assert.That(result.NetGp, Is.EqualTo(2_000m * 100m / 84.3m).Within(0.000001m));
    }

    [Test]
    public void Post99AndPersonalRatesKeepGoalCoinsWithoutTaxOrMarketPricing()
    {
        var calculator = new TrainingPlanCalculator();
        var definition = Definition();
        var result = calculator.Calculate(definition, 13_034_431, 200_000_000,
            new Dictionary<int, ItemPrice>(), methodId: "ardougne-knights");
        var slower = calculator.Calculate(definition, 13_034_431, 200_000_000,
            new Dictionary<int, ItemPrice>(), 126_450m, "ardougne-knights");

        Assert.That(result.Hours, Is.EqualTo(739.286552m).Within(0.000001m));
        Assert.That(result.AverageGpPerHour, Is.EqualTo(300_000m).Within(0.000001m));
        Assert.That(result.NetGp, Is.EqualTo(221_785_965.599m).Within(0.001m));
        Assert.That(result.GpPerExperience, Is.EqualTo(100m / 84.3m).Within(0.000001m));
        Assert.That(slower.Hours, Is.EqualTo(result.Hours * 2m).Within(0.000001m));
        Assert.That(slower.NetGp, Is.EqualTo(result.NetGp));
    }
}
