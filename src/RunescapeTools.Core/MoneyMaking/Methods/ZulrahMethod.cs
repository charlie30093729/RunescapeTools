using RunescapeTools.Core.MoneyMaking;

namespace RunescapeTools.Core.MoneyMaking.Methods
{
    public class ZulrahMethod : IMoneyMakingMethod
    {
        public MoneyMakingMethodDefinition Definition { get; } = new(
            Slug: "zulrah",
            Name: "Zulrah",
            Description: "Expected gp/hr using melee only at Zulrah.",
            ActionsPerHour: 31m,
            Accounts: 1,
            GrandExchangeTaxRate: 0.02m,
            Items:
            [
                // Supplies per hour
                new(23685, "Divine super combat potion(4)", 3m, ItemFlowDirection.Input),
                new(2434, "Prayer potion(4)", 16m, ItemFlowDirection.Input),
                new(12938, "Zul-andra teleport", 7m, ItemFlowDirection.Input),
                new(13441, "Anglerfish", 112m, ItemFlowDirection.Input),

                // Unique drops
                new(12922, "Tanzanite fang", 1m / 512m,
                    ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(12932, "Magic fang", 1m / 512m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(12927, "Serpentine visage", 1m / 512m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                // Guaranteed scales
                new(12934, "Zulrah's scales", 200m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                // Common resource drops
                new(560, "Death rune", 300m / 4m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(562, "Chaos rune", 500m / 4m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(563, "Law rune", 200m / 6m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(451, "Runite ore", 8m / 10m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(2361, "Adamantite bar", 25m / 8m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(6289, "Snakeskin", 35m / 6m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(1779, "Flax", 1000m / 6m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(1513, "Magic logs", 100m / 10m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(536, "Dragon bones", 30m / 8m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(8782, "Mahogany plank", 50m / 10m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(1939, "Swamp tar", 1000m / 6m,ItemFlowDirection.Output, QuantityBasis.PerAction),

                new(12938, "Zul-andra teleport", 1m / 3m, ItemFlowDirection.Output, QuantityBasis.PerAction)
            ]);
    }
}

