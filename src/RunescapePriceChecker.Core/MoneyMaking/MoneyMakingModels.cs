using RunescapePriceChecker.Core.Market;

namespace RunescapePriceChecker.Core.MoneyMaking;

public enum ItemFlowDirection
{
    Input,
    Output
}

public enum QuantityBasis
{
    PerHour,
    PerAction
}

public sealed record ItemFlow(
    int ItemId,
    string Name,
    decimal Quantity,
    ItemFlowDirection Direction,
    QuantityBasis Basis = QuantityBasis.PerHour,
    bool ApplyGrandExchangeTax = true);

public sealed record ExperienceReward(string Skill, decimal ExperiencePerAction);

public sealed record MoneyMakingMethodDefinition(
    string Slug,
    string Name,
    string Description,
    decimal ActionsPerHour,
    int Accounts,
    decimal GrandExchangeTaxRate,
    IReadOnlyList<ItemFlow> Items,
    IReadOnlyList<ExperienceReward>? ExperienceRewards = null)
{
    public IReadOnlyCollection<int> RequiredItemIds => Items
        .Select(item => item.ItemId)
        .Distinct()
        .ToArray();
}

public interface IMoneyMakingMethod
{
    MoneyMakingMethodDefinition Definition { get; }
}

public sealed record MoneyMakingLineResult(
    ItemFlow Item,
    decimal QuantityPerHour,
    decimal? UnitPrice,
    decimal GrossValuePerHour,
    decimal TaxPerHour)
{
    public bool HasPrice => UnitPrice.HasValue;
}

public sealed record ExperiencePerHourResult(string Skill, decimal ExperiencePerHour);

public sealed record MoneyMakingResult(
    MoneyMakingMethodDefinition Method,
    decimal GrossRevenuePerAccount,
    decimal TaxPerAccount,
    decimal InputCostPerAccount,
    decimal ProfitPerAccount,
    decimal ProfitAllAccounts,
    IReadOnlyList<MoneyMakingLineResult> Lines,
    IReadOnlyList<ExperiencePerHourResult> ExperiencePerHour)
{
    public bool HasMissingPrices => Lines.Any(line => !line.HasPrice);
}
