namespace RunescapeTools.Core.MoneyMaking.Methods;

/// <summary>
/// The original Vyrewatch calculation expressed as reusable item flows.
/// Outputs are expected quantities per kill; supplies are quantities used per hour.
/// </summary>
public sealed class VyrewatchMethod : IMoneyMakingMethod
{
    public MoneyMakingMethodDefinition Definition { get; } = new(
        Slug: "vyrewatch-sentinels",
        Name: "Vyrewatch Sentinels",
        Description: "Expected drop value minus hourly prayer and combat supplies, using the original five-account setup.",
        ActionsPerHour: 102m,
        Accounts: 5,
        GrandExchangeTaxRate: 0.02m,
        Items:
        [
            new(30125, "Prayer regeneration potion(4)", 2m, ItemFlowDirection.Input),
            new(12695, "Super combat potion(4)", 2m, ItemFlowDirection.Input),

            new(24777, "Blood shard", 1m / 1500m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(1123, "Adamant platebody", 1m / 128m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(1201, "Rune kiteshield", 1m / 128m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(2363, "Runite bar", 1m / 128m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(1163, "Rune full helm", 1m / 128m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(451, "Runite ore", 1m / 100m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(5295, "Ranarr seed", 1m / 106m, ItemFlowDirection.Output, QuantityBasis.PerAction),
            new(9194, "Onyx bolt tips", 12m / 128m, ItemFlowDirection.Output, QuantityBasis.PerAction)
        ]);
}
