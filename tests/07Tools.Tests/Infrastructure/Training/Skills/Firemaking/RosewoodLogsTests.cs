namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Firemaking;

[TestFixture]
[Category("Unit")]
public sealed class RosewoodLogsTests
{
    [Test]
    [Property("LegacyScenario", "DeterministicMethodCatalogue")]
    public void ReviewedRateAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var firemaking = TrainingBand(catalogue, "Firemaking", 13_034_431);

        Assert.That(firemaking.ExperiencePerHour, Is.EqualTo(623_700m * 1.025m).Within(0m), "Firemaking rate");

        Assert.That(firemaking.Method, Is.EqualTo("Rosewood logs - bow burning"), "Firemaking method");

        Assert.That(Resource(firemaking, 32910).QuantityPerExperience, Is.EqualTo(1m / (420m * 1.025m)).Within(0m), "rosewood logs per XP");
    }
}
