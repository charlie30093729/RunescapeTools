namespace RunescapeTools.Tests.Core.MoneyMaking;

[TestFixture]
[Category("Unit")]
public sealed class FrostDragonMethodTests
{
    [Test]
    [Property("LegacyScenario", "FrostDragonMethodDefinition")]
    [Description("Frost dragons expose reviewed 120-kill economics and optional bones")]
    public void FrostDragonMethodDefinition()
    {
        var withBones = new FrostDragonMethod().Definition;
        var withoutBones = FrostDragonMethod.CreateDefinition(pickUpFrostDragonBones: false);
        var prices = withBones.RequiredItemIds.ToDictionary(id => id, id => Quote(id, 1_000));
        var calculator = new MoneyMakingCalculator();
        var collected = calculator.Calculate(withBones, prices);
        var leftBehind = calculator.Calculate(withoutBones, prices);

        Assert.That(withBones.ActionsPerHour, Is.EqualTo(120m).Within(0m), "default Frost Dragon kills per hour");
        Assert.That(withBones.Accounts, Is.EqualTo(1), "default Frost Dragon account count");
        Assert.That(withBones.RequiredItemIds.Count, Is.EqualTo(withBones.Items.Count), "Frost Dragon item IDs are distinct");
        Assert.That(Quantity(collected, "Prayer potion(4)"), Is.EqualTo(16m).Within(0m), "hourly prayer potions");
        Assert.That(Quantity(collected, "Divine super combat potion(4)"), Is.EqualTo(3m).Within(0m), "hourly combat potions");
        Assert.That(Quantity(collected, "Extended super antifire(4)"), Is.EqualTo(3m).Within(0m), "hourly antifires");
        Assert.That(Quantity(collected, "Teleport to house (tablet)"), Is.EqualTo(7.2m).Within(0m), "hourly house teleports");
        Assert.That(Quantity(collected, "Frost dragon bones"), Is.EqualTo(120m).Within(0m), "hourly Frost dragon bones");
        Assert.That(Quantity(collected, "Dragon metal sheet"), Is.EqualTo(1.2m).Within(0m), "off-task metal sheets");
        Assert.That(Quantity(collected, "Dragon nails"), Is.EqualTo(360m / 13m).Within(0.0000001m), "expected dragon nails");
        Assert.That(withoutBones.Items.All(item => item.ItemId != 31729), Is.True, "disabled bone collection removes Frost dragon bones from the ledger");
        Assert.That(collected.ProfitPerAccount - leftBehind.ProfitPerAccount, Is.EqualTo(120m * 1_000m * 0.98m).Within(0m), "bone collection contributes taxed bone revenue");

        var customRate = calculator.Calculate(withBones with { ActionsPerHour = 100m }, prices);
        Assert.That(Quantity(customRate, "Frost dragon bones"), Is.EqualTo(100m).Within(0m), "custom rate scales per-kill loot");
        Assert.That(Quantity(customRate, "Prayer potion(4)"), Is.EqualTo(16m).Within(0m), "custom rate preserves hourly prayer supplies");
    }
}
