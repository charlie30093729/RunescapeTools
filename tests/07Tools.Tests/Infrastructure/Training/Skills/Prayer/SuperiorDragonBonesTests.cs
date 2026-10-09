namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Prayer;

[TestFixture]
[Category("Unit")]
public sealed class SuperiorDragonBonesTests
{
    [Test]
    [Property("LegacyScenario", "DeterministicMethodCatalogue")]
    public void ReviewedRateAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var prayer = TrainingBand(catalogue, "Prayer", 0);

        Assert.That(prayer.ExperiencePerHour, Is.EqualTo(2_000_000m).Within(0m), "Prayer rate");

        Assert.That(prayer.Method, Is.EqualTo("Superior dragon bones at the Gilded Altar"), "Prayer method");

        Assert.That(Resource(prayer, 22124).QuantityPerExperience, Is.EqualTo(1m / 525m).Within(0m), "Prayer bones per XP");
    }
}
