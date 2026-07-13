using RunescapePriceChecker.Core.Market;

namespace RunescapePriceChecker.Core.MoneyMaking;

public sealed class MoneyMakingCalculator
{
    public MoneyMakingResult Calculate(
        MoneyMakingMethodDefinition method,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(prices);

        var lines = method.Items.Select(item => CalculateLine(method, item, prices)).ToArray();

        var grossRevenue = lines
            .Where(line => line.Item.Direction == ItemFlowDirection.Output)
            .Sum(line => line.GrossValuePerHour);
        var tax = lines.Sum(line => line.TaxPerHour);
        var inputCost = lines
            .Where(line => line.Item.Direction == ItemFlowDirection.Input)
            .Sum(line => line.GrossValuePerHour);
        var profitPerAccount = grossRevenue - tax - inputCost;

        var experience = (method.ExperienceRewards ?? [])
            .Select(reward => new ExperiencePerHourResult(
                reward.Skill,
                reward.ExperiencePerAction * method.ActionsPerHour))
            .ToArray();

        return new MoneyMakingResult(
            method,
            grossRevenue,
            tax,
            inputCost,
            profitPerAccount,
            profitPerAccount * method.Accounts,
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
