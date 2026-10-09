namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Smithing;

[TestFixture]
[Category("Unit")]
public sealed class BlastFurnaceGoldTests
{
    [Test]
    [Property("LegacyScenario", "DeterministicMethodCatalogue")]
    public void ReviewedRateAndItemFlows()
    {
        var catalogue = new MainEhpCatalogue();

        var smithing = TrainingBand(catalogue, "Smithing", 13_034_431);

        Assert.That(smithing.ExperiencePerHour, Is.EqualTo(410_000m).Within(0m), "Smithing 99+ rate");

        Assert.That(smithing.Method, Is.EqualTo("Solo Blast Furnace gold"), "Smithing method");

        Assert.That(smithing.Economics!.FixedGpPerHour, Is.EqualTo(72_000m).Within(0m), "Blast Furnace hourly fee");

        Assert.That(Resource(smithing, 12625).QuantityPerHour, Is.EqualTo(10m).Within(0m), "stamina potions per hour");
    }
}
