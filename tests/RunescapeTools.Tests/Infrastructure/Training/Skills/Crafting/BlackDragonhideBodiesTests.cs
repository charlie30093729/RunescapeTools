namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Crafting;

[TestFixture]
[Category("Unit")]
public sealed class BlackDragonhideBodiesTests
{
    [Test]
    [Property("LegacyScenario", "DeterministicMethodCatalogue")]
    public void ReviewedRateAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var crafting = TrainingBand(catalogue, "Crafting", 2_951_373);

        Assert.That(crafting.ExperiencePerHour, Is.EqualTo(465_000m).Within(0m), "Crafting rate");

        Assert.That(crafting.Method, Is.EqualTo("Black dragonhide bodies"), "Crafting method");

        Assert.That(Resource(crafting, 2509).QuantityPerExperience, Is.EqualTo(3m / 258m).Within(0m), "black leather per XP");

        Assert.That(Resource(crafting, 2503).QuantityPerExperience, Is.EqualTo(1m / 258m).Within(0m), "black bodies per XP");
    }
}
