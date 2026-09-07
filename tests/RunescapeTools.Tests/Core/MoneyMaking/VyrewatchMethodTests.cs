namespace RunescapeTools.Tests.Core.MoneyMaking;

[TestFixture]
[Category("Unit")]
public sealed class VyrewatchMethodTests
{
    [Test]
    [Property("LegacyScenario", "VyrewatchMatchesLegacyFormula")]
    [Description("Vyrewatch defaults to the no-regen formula")]
    public void VyrewatchMatchesLegacyFormula()
    {
        var method = new VyrewatchMethod().Definition;
        var prices = method.RequiredItemIds.ToDictionary(id => id, id => Quote(id, 1_000));
        var expectedOutputQuantityPerKill = (1m / 1500m) + (4m / 128m) + (1m / 100m) + (1m / 106m) + (12m / 128m);
        var expectedGross = expectedOutputQuantityPerKill * 88m * 1_000m;
        var expectedSupplies = 2m * 1_000m;
        var expectedProfit = expectedGross * 0.98m - expectedSupplies;

        var result = new MoneyMakingCalculator().Calculate(method, prices);

        Assert.That(result.GrossRevenuePerAccount, Is.EqualTo(expectedGross).Within(0.0001m), "legacy gross output");
        Assert.That(result.InputCostPerAccount, Is.EqualTo(expectedSupplies).Within(0m), "legacy hourly supplies");
        Assert.That(result.ProfitPerAccount, Is.EqualTo(expectedProfit).Within(0.0001m), "legacy profit");
        Assert.That(result.ProfitAllAccounts, Is.EqualTo(expectedProfit * 5m).Within(0.0001m), "legacy multi-account profit");
        Assert.That(method.Items.All(item => item.ItemId != 30125), Is.True, "default ledger excludes prayer regeneration potions");
    }

    [Test]
    [Property("LegacyScenario", "VyrewatchNoRegenConfiguration")]
    [Description("Vyrewatch supports the regen-potion configuration")]
    public void RegenConfigurationIncludesPotionsAndHigherKillRate()
    {
        var method = VyrewatchMethod.CreateDefinition(usingRegenPotions: true);
        var prices = new VyrewatchMethod().Definition.RequiredItemIds
            .Append(30125)
            .ToDictionary(id => id, id => Quote(id, 1_000));
        var result = new MoneyMakingCalculator().Calculate(method, prices);

        Assert.That(method.ActionsPerHour, Is.EqualTo(102m).Within(0m), "regen kills per hour");
        Assert.That(method.Items.Any(item => item.ItemId == 30125), Is.True, "regen configuration includes prayer regeneration potions");
        Assert.That(result.InputCostPerAccount, Is.EqualTo(4_000m).Within(0m), "regen hourly supplies");
        Assert.That(result.Lines.Any(line => line.Item.ItemId == 30125), Is.True, "regen ledger includes prayer regeneration potions");
    }

    [Test]
    [Property("LegacyScenario", "VyrewatchItemIdsAreDistinct")]
    [Description("Vyrewatch exposes every required item once")]
    public void VyrewatchItemIdsAreDistinct()
    {
        var method = new VyrewatchMethod().Definition;
        Assert.That(method.RequiredItemIds.Count, Is.EqualTo(9m).Within(0m), "required item count");
        Assert.That(method.Items.Select(item => item.ItemId).Distinct().Count(), Is.EqualTo(9m).Within(0m), "unique item count");
    }
}
