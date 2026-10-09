namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Fletching;

[TestFixture]
[Category("Unit")]
public sealed class FletchingConfigurationTests
{
    [Test]
    [Property("LegacyScenario", "TrainingSkillConfiguration")]
    public void HoursCanBeExcluded()
    {
        var catalogue = new MainEhpCatalogue();

        var calculator = new TrainingPlanCalculator();

        var prices = new Dictionary<int, ItemPrice>();

        var fletching = catalogue.Skills.Single(skill => skill.Skill == "Fletching");

        var counted = calculator.Calculate(fletching, 5_346_332, 6_346_332, prices);

        var zeroTime = calculator.Calculate(
            fletching,
            5_346_332,
            6_346_332,
            prices,
            configuration: new Dictionary<string, string>
            {
                ["include-hours"] = bool.FalseString
            });

        Assert.That(counted.Hours, Is.EqualTo(1m).Within(0m), "Fletching default active hours");

        Assert.That(zeroTime.Hours, Is.EqualTo(0m).Within(0m), "Fletching hidden active hours");

        Assert.That(!zeroTime.IncludesActiveHours, Is.True, "Fletching zero-time flag");
    }
}
