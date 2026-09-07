namespace RunescapeTools.Tests.Core.MoneyMaking;

[TestFixture]
[Category("Unit")]
public sealed class RuneDragonMethodTests
{
    [Test]
    [Property("LegacyScenario", "RuneDragonMethodDefinition")]
    [Description("Rune dragons expose reviewed 45-kill economics")]
    public void RuneDragonMethodDefinition()
    {
        var method = new RuneDragonMethod().Definition;
        var prices = method.RequiredItemIds.ToDictionary(id => id, id => Quote(id, 1_000));
        var result = new MoneyMakingCalculator().Calculate(method, prices);

        Assert.That(method.ActionsPerHour, Is.EqualTo(45m).Within(0m), "default rune-dragon kills per hour");
        Assert.That(method.Accounts, Is.EqualTo(1), "default rune-dragon account count");
        Assert.That(method.RequiredItemIds.Count, Is.EqualTo(method.Items.Count), "rune-dragon item IDs are distinct");
        const decimal quantityTolerance = 0.0000001m;
        Assert.That(Quantity(result, "Prayer potion(4)"), Is.EqualTo(15m).Within(quantityTolerance), "hourly prayer potions");
        Assert.That(Quantity(result, "Divine super combat potion(4)"), Is.EqualTo(2.5m).Within(quantityTolerance), "hourly combat potions");
        Assert.That(Quantity(result, "Extended antifire(4)"), Is.EqualTo(1.25m).Within(quantityTolerance), "hourly antifires");
        Assert.That(Quantity(result, "Cooked karambwan"), Is.EqualTo(100m).Within(quantityTolerance), "hourly food");
        Assert.That(Quantity(result, "Teleport to house (tablet)"), Is.EqualTo(5m).Within(quantityTolerance), "hourly house teleports");
        Assert.That(Quantity(result, "Runite bar"), Is.EqualTo(45m).Within(quantityTolerance), "guaranteed hourly runite bars");
        Assert.That(Quantity(result, "Dragon bones"), Is.EqualTo(45m).Within(quantityTolerance), "guaranteed hourly dragon bones");
        Assert.That(method.Items.Any(item => item.ItemId == 19580 && item.Name == "Rune javelin tips"), Is.True, "current rune-javelin item name and ID");
        Assert.That(method.Items.Any(item => item.ItemId == 19582 && item.Name == "Dragon javelin tips"), Is.True, "current dragon-javelin item name and ID");

        var faster = new MoneyMakingCalculator().Calculate(
            method with { ActionsPerHour = 50m },
            prices);
        Assert.That(Quantity(faster, "Runite bar"), Is.EqualTo(50m).Within(quantityTolerance), "custom rate scales outputs");
        Assert.That(Quantity(faster, "Prayer potion(4)"), Is.EqualTo(50m / 3m).Within(quantityTolerance), "custom rate scales supplies");
    }
}
