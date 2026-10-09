namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Cooking;

[TestFixture]
[Category("Unit")]
public sealed class SummerPiesTests
{
    [Test]
    [Property("LegacyScenario", "DeterministicMethodCatalogue")]
    public void ReviewedRateAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var cooking = TrainingBand(catalogue, "Cooking", 8_771_558);

        Assert.That(cooking.ExperiencePerHour, Is.EqualTo(490_000m).Within(0m), "Cooking rate");

        Assert.That(cooking.Method, Is.EqualTo("Bake Pie spell - summer pies"), "Cooking method");

        Assert.That(string.Join('|', cooking.Economics!.Resources.Select(resource => resource.ItemId).Order()), Is.EqualTo("7216|7218|9075"), "Cooking item IDs");

        Assert.That(Resource(cooking, 7216).QuantityPerExperience, Is.EqualTo(1m / 260m).Within(0m), "raw summer pies per XP");
    }
}
