using RunescapeTools.Core.Market;

namespace RunescapeTools.Core.MoneyMaking;

public sealed class MoneyMakingCalculator
{
    public MoneyMakingResult Calculate(
        MoneyMakingMethodDefinition method,
        IReadOnlyDictionary<int, ItemPrice> prices,
        int? accountCount = null)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(prices);

        var effectiveAccounts = accountCount ?? method.Accounts;
        if (effectiveAccounts < 1)
            throw new ArgumentOutOfRangeException(nameof(accountCount), "Account count must be at least one.");

        var effectiveMethod = method with { Accounts = effectiveAccounts };
        var lines = effectiveMethod.Items
            .Select(item => CalculateLine(effectiveMethod, item, prices))
            .ToArray();

        var grossRevenue = lines
            .Where(line => line.Item.Direction == ItemFlowDirection.Output)
            .Sum(line => line.GrossValuePerHour);
        var tax = lines.Sum(line => line.TaxPerHour);
        var inputCost = lines
            .Where(line => line.Item.Direction == ItemFlowDirection.Input)
            .Sum(line => line.GrossValuePerHour);
        var profitPerAccount = grossRevenue - tax - inputCost;

        var experience = (effectiveMethod.ExperienceRewards ?? [])
            .Select(reward => new ExperiencePerHourResult(
                reward.Skill,
                reward.ExperiencePerAction * effectiveMethod.ActionsPerHour))
            .ToArray();

        return new MoneyMakingResult(
            effectiveMethod,
            grossRevenue,
            tax,
            inputCost,
            profitPerAccount,
            profitPerAccount * effectiveAccounts,
            lines,
            experience);
    }

    private static MoneyMakingLineResult CalculateLine(
        MoneyMakingMethodDefinition method,
        ItemFlow item,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
        var quantityPerHour = item.Basis == QuantityBasis.PerAction
            ? item.Quantity * method.ActionsPerHour
            : item.Quantity;

        var price = prices.TryGetValue(item.ItemId, out var quote)
            ? quote.MidPrice
            : null;
        var value = quantityPerHour * (price ?? 0m);
        var tax = item.Direction == ItemFlowDirection.Output && item.ApplyGrandExchangeTax
            ? value * method.GrandExchangeTaxRate
            : 0m;

        return new MoneyMakingLineResult(item, quantityPerHour, price, value, tax);
    }
}
