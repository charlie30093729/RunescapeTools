namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Fletching;

[TestFixture]
[Category("Unit")]
public sealed class AmethystDartsTests
{
    [Test]
    [Property("LegacyScenario", "DeterministicMethodCatalogue")]
    public void ReviewedRateAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var fletching = TrainingBand(catalogue, "Fletching", 5_346_332);

        Assert.That(fletching.ExperiencePerHour, Is.EqualTo(1_000_000m).Within(0m), "Fletching rate");

        Assert.That(fletching.Method, Is.EqualTo("Amethyst darts"), "Fletching method");

        Assert.That(Resource(fletching, 25853).QuantityPerExperience, Is.EqualTo(1m / 21m).Within(0m), "amethyst tips per XP");
    }
}
