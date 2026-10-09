namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Construction;

[TestFixture]
[Category("Unit")]
public sealed class ConstructionConfigurationTests
{
    [Test]
    [Property("LegacyScenario", "TrainingSkillConfiguration")]
    public void CarpenterOutfitAdjustsRateAndMaterials()
    {
        var catalogue = new MainEhpCatalogue();

        var calculator = new TrainingPlanCalculator();

        var prices = new Dictionary<int, ItemPrice>();

        var construction = catalogue.Skills.Single(skill => skill.Skill == "Construction");

        var carpenter = calculator.Calculate(
            construction,
            13_034_431,
            14_474_431,
            prices,
            configuration: new Dictionary<string, string>
            {
                ["carpenters-outfit"] = bool.TrueString
            });

        Assert.That(carpenter.BaseRate, Is.EqualTo(1_440_000m * 1.025m).Within(0m), "Carpenter outfit rate");

        Assert.That(Resource(carpenter.Method.Bands[^1], 8782).QuantityPerExperience, Is.EqualTo(1m / 140m / 1.025m).Within(0m), "Carpenter outfit plank consumption");
    }
}
