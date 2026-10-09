namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Runecraft;

[TestFixture]
[Category("Unit")]
public sealed class SoloMudRunesTests
{
    [Test]
    [Property("LegacyScenario", "PhaseTwoMethodCatalogue")]
    public void ReviewedRatesAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var runecraft75 = TrainingBand(catalogue, "Runecraft", 1_210_421);

        Assert.That(runecraft75.ExperiencePerHour, Is.EqualTo(74_500m).Within(0m), "Runecraft level-75 rate");

        Assert.That(Resource(runecraft75, 7936).QuantityPerExperience, Is.EqualTo(50m / 475m).Within(0m), "level-75 essence per XP");

        Assert.That(Resource(runecraft75, 4698).QuantityPerExperience, Is.EqualTo(85m / 475m).Within(0m), "level-75 mud runes per XP");

        Assert.That(Resource(runecraft75, 5521).QuantityPerExperience, Is.EqualTo(0.2m / 475m).Within(0m), "level-75 necklaces per XP");

        Assert.That(Resource(runecraft75, 9075).QuantityPerExperience, Is.EqualTo(2.1m / 475m).Within(0m), "level-75 astrals per XP");

        var runecraft85 = TrainingBand(catalogue, "Runecraft", 3_258_594);

        Assert.That(runecraft85.ExperiencePerHour, Is.EqualTo(96_900m).Within(0m), "Runecraft level-85 rate");

        Assert.That(Resource(runecraft85, 7936).QuantityPerExperience, Is.EqualTo(63m / 598.5m).Within(0m), "level-85 essence per XP");

        Assert.That(Resource(runecraft85, 4698).QuantityPerExperience, Is.EqualTo(105.3m / 598.5m).Within(0m), "level-85 mud runes per XP");

        Assert.That(Resource(runecraft85, 9075).QuantityPerExperience, Is.EqualTo(2.125m / 598.5m).Within(0m), "level-85 astrals per XP");

        Assert.That(Resource(runecraft85, 556).QuantityPerExperience, Is.EqualTo(0.25m / 598.5m).Within(0m), "level-85 air runes per XP");

        Assert.That(Resource(runecraft85, 564).QuantityPerExperience, Is.EqualTo(0.125m / 598.5m).Within(0m), "level-85 cosmic runes per XP");

        var runecraft99 = TrainingBand(catalogue, "Runecraft", 13_034_431);

        Assert.That(runecraft99.ExperiencePerHour, Is.EqualTo(98_200m).Within(0m), "Runecraft level-99 rate");

        Assert.That(runecraft99.Method, Is.EqualTo("Solo mud runes"), "Runecraft method");

        Assert.That(runecraft99.Economics!.Resources.All(resource => resource.ItemId is not 556 and not 564), Is.True, "Runecraft cape should remove NPC Contact rune costs");

        Assert.That(Resource(runecraft99, 9075).QuantityPerExperience, Is.EqualTo(2m / 598.5m).Within(0m), "level-99 Magic Imbue astrals");
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

        var runecraft = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Runecraft"),
            0,
            TrainingPlanCalculator.MaximumExperience,
            prices);

        Assert.That(runecraft.PricedExperience, Is.EqualTo(198_789_579L), "priced Runecraft XP");

        Assert.That(runecraft.NetGp > 0m, Is.True, "solo mud runes should produce profit with the test market");

        Assert.That(!runecraft.HasMissingPrice, Is.True, "all solo mud-rune resources should be priced");

        Assert.That(!runecraft.IsFullyPriced, Is.True, "early Runecraft should remain visibly unpriced");
    }
}
