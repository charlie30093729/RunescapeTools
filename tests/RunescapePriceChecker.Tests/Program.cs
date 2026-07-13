using RunescapePriceChecker.Core.Market;
using RunescapePriceChecker.Core.MoneyMaking;
using RunescapePriceChecker.Core.MoneyMaking.Methods;

var tests = new (string Name, Action Run)[]
{
    ("generic flow calculation", GenericFlowCalculation),
    ("Vyrewatch matches the legacy formula", VyrewatchMatchesLegacyFormula),
    ("Vyrewatch exposes every required item once", VyrewatchItemIdsAreDistinct),
    ("mid price falls back to the available quote", MidPriceFallback)
};

var failures = new List<string>();
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS  {test.Name}");
    }
    catch (Exception exception)
    {
        failures.Add(test.Name);
        Console.WriteLine($"FAIL  {test.Name}: {exception.Message}");
    }
}

Console.WriteLine();
Console.WriteLine($"{tests.Length - failures.Count}/{tests.Length} checks passed.");
return failures.Count == 0 ? 0 : 1;

static void GenericFlowCalculation()
{
    var method = new MoneyMakingMethodDefinition(
        "test",
        "Test",
        "Known values",
        ActionsPerHour: 10m,
        Accounts: 2,
        GrandExchangeTaxRate: 0.02m,
        Items:
        [
            new ItemFlow(1, "Input", 3m, ItemFlowDirection.Input),
            new ItemFlow(2, "Output", 0.5m, ItemFlowDirection.Output, QuantityBasis.PerAction)
        ]);
    var prices = new Dictionary<int, ItemPrice>
    {
        [1] = Quote(1, 100),
        [2] = Quote(2, 1000)
    };

    var result = new MoneyMakingCalculator().Calculate(method, prices);

    Equal(5_000m, result.GrossRevenuePerAccount, "gross revenue");
    Equal(100m, result.TaxPerAccount, "tax");
    Equal(300m, result.InputCostPerAccount, "input cost");
    Equal(4_600m, result.ProfitPerAccount, "profit per account");
    Equal(9_200m, result.ProfitAllAccounts, "profit for all accounts");
}

static void VyrewatchMatchesLegacyFormula()
{
    var method = new VyrewatchMethod().Definition;
    var prices = method.RequiredItemIds.ToDictionary(id => id, id => Quote(id, 1_000));

    var expectedOutputQuantityPerKill =
        (1m / 1500m)
        + (4m / 128m)
        + (1m / 100m)
        + (1m / 106m)
        + (12m / 128m);
    var expectedGross = expectedOutputQuantityPerKill * 102m * 1_000m;
    var expectedSupplies = 4m * 1_000m;
    var expectedProfit = expectedGross * 0.98m - expectedSupplies;

    var result = new MoneyMakingCalculator().Calculate(method, prices);

    Equal(expectedGross, result.GrossRevenuePerAccount, "legacy gross output", tolerance: 0.0001m);
    Equal(expectedSupplies, result.InputCostPerAccount, "legacy hourly supplies");
    Equal(expectedProfit, result.ProfitPerAccount, "legacy profit", tolerance: 0.0001m);
    Equal(expectedProfit * 5m, result.ProfitAllAccounts, "legacy multi-account profit", tolerance: 0.0001m);
}

static void VyrewatchItemIdsAreDistinct()
{
    var method = new VyrewatchMethod().Definition;
    Equal(10m, method.RequiredItemIds.Count, "required item count");
    Equal(10m, method.Items.Select(item => item.ItemId).Distinct().Count(), "unique item count");
}

static void MidPriceFallback()
{
    var highOnly = new ItemPrice(1, 777, null, null, null);
    var lowOnly = new ItemPrice(2, null, 555, null, null);
    Equal(777m, highOnly.MidPrice ?? 0, "high-only midpoint");
    Equal(555m, lowOnly.MidPrice ?? 0, "low-only midpoint");
}

static ItemPrice Quote(int itemId, long value) => new(itemId, value, value, null, null);

static void Equal(decimal expected, decimal actual, string label, decimal tolerance = 0m)
{
    if (Math.Abs(expected - actual) > tolerance)
        throw new InvalidOperationException($"{label}: expected {expected}, actual {actual}");
}
