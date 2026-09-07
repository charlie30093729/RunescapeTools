namespace RunescapeTools.Tests.Wpf.Dialogs;

[TestFixture]
[Category("Unit")]
public sealed class TrainingPriceDialogViewModelTests
{
    [Test]
    [Property("LegacyScenario", "XpPlannerPriceDialog")]
    [Description("XP Planner price dialog uses live offers and goal quantities")]
    public void XpPlannerPriceDialog()
    {
        var timestamp = new DateTimeOffset(2026, 7, 27, 5, 30, 0, TimeSpan.Zero);
        var prices = new Dictionary<int, ItemPrice>
        {
            [3002] = new ItemPrice(3002, 10_000, 9_000, timestamp, timestamp.AddMinutes(-1)),
            [6693] = new ItemPrice(6693, 5_000, 4_500, timestamp.AddMinutes(-2), timestamp.AddMinutes(-3)),
            [21163] = new ItemPrice(21163, 2_000, 1_800, timestamp.AddMinutes(-4), timestamp.AddMinutes(-5)),
            [6685] = new ItemPrice(6685, 12_000, 11_000, timestamp.AddMinutes(-6), timestamp.AddMinutes(-7))
        };
        var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Herblore");
        var row = new XpPlannerRowViewModel(
            definition,
            new TrainingPlanCalculator(),
            13_034_431,
            null,
            prices,
            () => { });

        Assert.That(row.Method, Is.EqualTo("Saradomin brews"), "active Herblore method");
        var dialog = new TrainingPriceDialogViewModel(row.Skill, row.Result, prices);
        var unfinishedPotion = dialog.Items.Single(item => item.Name == "Toadflax potion (unf)");
        var crushedNest = dialog.Items.Single(item => item.Name == "Crushed nest");
        var amulet = dialog.Items.Single(item => item.Name == "Amulet of chemistry");
        var finishedPotion = dialog.Items.Single(item => item.Name == "Saradomin brew(4)");
        Assert.That(unfinishedPotion.Action, Is.EqualTo("BUY"), "base potion action");
        Assert.That(unfinishedPotion.UnitPrice, Is.EqualTo("10,000 gp"), "base potion uses latest high");
        Assert.That(unfinishedPotion.QuoteDetail, Is.EqualTo("high - 2026-07-27 05:30 UTC"), "base potion quote details");
        Assert.That(crushedNest.UnitPrice, Is.EqualTo("5,000 gp"), "secondary ingredient uses latest high");
        Assert.That(amulet.UnitPrice, Is.EqualTo("2,000 gp"), "equipment charge input uses latest high");
        Assert.That(finishedPotion.Action, Is.EqualTo("SELL"), "finished potion action");
        Assert.That(finishedPotion.UnitPrice, Is.EqualTo("11,000 gp"), "finished potion uses latest low");
        Assert.That(finishedPotion.QuoteDetail, Is.EqualTo("low - 2026-07-27 05:23 UTC"), "finished potion quote details");
        var crushedNestRequirement = row.Result.ResourceRequirements.Single(
            item => item.ItemId == 6693 && item.Direction == TrainingFlowDirection.Input);
        Assert.That(crushedNest.Quantity, Is.EqualTo(Math.Ceiling(crushedNestRequirement.Quantity).ToString("N0")), "dialog displays the complete goal quantity");
        Assert.That(crushedNest.QuantityCaption, Is.EqualTo("required for goal"), "input quantity caption");
        Assert.That(finishedPotion.QuantityCaption, Is.EqualTo("expected output"), "output quantity caption");
        Assert.That(finishedPotion.Quantity.StartsWith("~ ", StringComparison.Ordinal), Is.True, "expected outputs are visibly approximate");
        Assert.That(dialog.GoalSummary.Contains("200,000,000", StringComparison.Ordinal), Is.True, "dialog identifies the goal XP");
        Assert.That(!string.IsNullOrWhiteSpace(dialog.RouteSummary), Is.True, "dialog identifies the calculated route");
        Assert.That(dialog.GpPerExperience, Is.EqualTo($"{row.Result.GpPerExperience:N2} gp/xp"), "dialog displays the calculated skill GP/XP");

        var fallbackPrices = new Dictionary<int, ItemPrice>(prices)
        {
            [6693] = new ItemPrice(6693, null, 4_500, null, timestamp.AddMinutes(-3))
        };
        row.UpdatePrices(fallbackPrices);
        dialog = new TrainingPriceDialogViewModel(row.Skill, row.Result, fallbackPrices);
        crushedNest = dialog.Items.Single(item => item.Name == "Crushed nest");
        Assert.That(crushedNest.UnitPrice, Is.EqualTo("4,500 gp"), "missing buy quote uses low fallback");
        Assert.That(crushedNest.QuoteDetail, Is.EqualTo("low fallback - 2026-07-27 05:27 UTC"), "fallback quote is disclosed");

        var outputFallback = TrainingMarketPricing.Select(
            TrainingFlowDirection.Output,
            new ItemPrice(6685, 12_000, null, timestamp, null));
        Assert.That(outputFallback.UnitPrice ?? 0L, Is.EqualTo(12_000L), "output high fallback price");
        Assert.That(outputFallback.UsedFallbackPrice, Is.True, "output high fallback state");

        var missingPrices = new Dictionary<int, ItemPrice>(prices);
        missingPrices.Remove(21163);
        row.UpdatePrices(missingPrices);
        dialog = new TrainingPriceDialogViewModel(row.Skill, row.Result, missingPrices);
        amulet = dialog.Items.Single(item => item.Name == "Amulet of chemistry");
        Assert.That(amulet.UnitPrice, Is.EqualTo("Unavailable"), "missing ingredient price is visible");
        Assert.That(amulet.QuoteDetail, Is.EqualTo("No high or low quote available"), "missing quote state is explained");
        Assert.That(dialog.GpPerExperience, Is.EqualTo("Unavailable"), "unpriced route does not report a zero GP/XP");
    }

    [Test]
    [Property("LegacyScenario", "XpPlannerPriceDialogIcons")]
    [Description("XP Planner price rows load cached item icons without changing item data")]
    public async Task XpPlannerPriceDialogIcons()
    {
        var prices = new Dictionary<int, ItemPrice>
        {
            [3002] = Quote(3002, 2_000),
            [6693] = Quote(6693, 4_000),
            [21163] = Quote(21163, 1_000),
            [6685] = Quote(6685, 6_000)
        };
        var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Herblore");
        var result = new TrainingPlanCalculator().Calculate(definition, 13_034_431, 200_000_000, prices);
        var iconPath = Path.Combine(Path.GetTempPath(), "toadflax.png");
        var icons = new FakeItemIconService(
            new ItemIcon(3002, "Toadflax potion (unf).png", iconPath));
        var dialog = new TrainingPriceDialogViewModel("Herblore", result, prices, icons);

        await dialog.LoadIconsAsync();

        var potion = dialog.Items.Single(item => item.ItemId == 3002);
        Assert.That(potion.IconPath ?? string.Empty, Is.EqualTo(iconPath), "resolved icon path");
        Assert.That(potion.Name, Is.EqualTo("Toadflax potion (unf)"), "item presentation remains intact");
        Assert.That(dialog.Items.Where(item => item.ItemId != 3002).All(item => item.IconPath is null), Is.True, "missing icons leave their rows readable");
    }
}
