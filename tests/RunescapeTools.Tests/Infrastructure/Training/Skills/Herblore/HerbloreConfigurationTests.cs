namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Herblore;

[TestFixture]
[Category("Unit")]
public sealed class HerbloreConfigurationTests
{
    [Test]
    [Property("LegacyScenario", "TrainingSkillConfiguration")]
    public void DisablingEquipmentRestoresBaseConsumption()
    {
        var catalogue = new MainEhpCatalogue();

        var calculator = new TrainingPlanCalculator();

        var prices = new Dictionary<int, ItemPrice>();

        var herblore = catalogue.Skills.Single(skill => skill.Skill == "Herblore");

        var noEquipment = calculator.Calculate(
            herblore,
            2_192_818,
            2_642_818,
            prices,
            configuration: new Dictionary<string, string>
            {
                ["prescription-goggles"] = bool.FalseString,
                ["alchemists-amulet"] = bool.FalseString
            });

        var brewBand = noEquipment.Method.Bands[^1];

        Assert.That(Resource(brewBand, 6693).QuantityPerExperience, Is.EqualTo(1m / 180m).Within(0m), "Herblore without goggles");

        Assert.That(brewBand.Economics!.Resources.All(resource => resource.ItemId != 21163), Is.True, "Herblore without amulet has no charge input");

        Assert.That(Resource(brewBand, 6685).QuantityPerExperience, Is.EqualTo(3m / 4m / 180m).Within(0m), "Herblore without amulet has base output doses");
    }
}
