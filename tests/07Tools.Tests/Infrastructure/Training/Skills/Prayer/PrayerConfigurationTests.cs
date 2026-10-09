namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Prayer;

[TestFixture]
[Category("Unit")]
public sealed class PrayerConfigurationTests
{
    [Test]
    [Property("LegacyScenario", "TrainingSkillConfiguration")]
    public void AltarChangesBoneConsumption()
    {
        var catalogue = new MainEhpCatalogue();

        var calculator = new TrainingPlanCalculator();

        var prices = new Dictionary<int, ItemPrice>();

        var prayer = catalogue.Skills.Single(skill => skill.Skill == "Prayer");

        var gilded = calculator.Calculate(
            prayer,
            0,
            1_000_000,
            prices);

        Assert.That(gilded.Method.Bands[0].Method, Is.EqualTo("Superior dragon bones at the Gilded Altar"), "Prayer defaults to Gilded Altar");

        Assert.That(Resource(gilded.Method.Bands[0], 22124).QuantityPerExperience, Is.EqualTo(1m / 525m).Within(0m), "Gilded Altar superior bone consumption");

        var chaos = calculator.Calculate(
            prayer,
            0,
            1_000_000,
            prices,
            configuration: new Dictionary<string, string>
            {
                ["offering-location"] = "chaos-altar"
            });

        Assert.That(chaos.Method.Bands[0].Method, Is.EqualTo("Superior dragon bones at the Chaos Altar"), "Prayer Chaos Altar selection");

        Assert.That(Resource(chaos.Method.Bands[0], 22124).QuantityPerExperience, Is.EqualTo(1m / 1_050m).Within(0m), "Chaos Altar superior bone consumption");
    }
}
