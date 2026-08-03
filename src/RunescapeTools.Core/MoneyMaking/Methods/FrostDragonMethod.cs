namespace RunescapeTools.Core.MoneyMaking.Methods;

/// <summary>
/// Off-task, low-interaction melee Frost Dragons. Consumable supplies are hourly;
/// expected drops are per kill so personal kill-rate overrides scale the loot ledger.
/// </summary>
public sealed class FrostDragonMethod : IMoneyMakingMethod
{
    public const string Slug = "frost-dragons-afk-melee";
    public const string PickUpBonesOptionKey = "pick-up-frost-dragon-bones";
    public const decimal DefaultKillsPerHour = 120m;

    public MoneyMakingMethodDefinition Definition { get; } =
        CreateDefinition(pickUpFrostDragonBones: true);

    public static MoneyMakingMethodDefinition CreateDefinition(bool pickUpFrostDragonBones)
    {
        var items = new List<ItemFlow>
        {
            // Hourly supplies for Protect from Melee, Piety, and full dragonfire immunity.
            InputPerHour(2434, "Prayer potion(4)", 16m),
            InputPerHour(23685, "Divine super combat potion(4)", 3m),
            InputPerHour(22209, "Extended super antifire(4)", 3m),
            InputPerKill(8013, "Teleport to house (tablet)", 0.06m)
        };

        if (pickUpFrostDragonBones)
            items.Add(OutputPerKill(31729, "Frost dragon bones", 1m));

        items.AddRange(
        [
            // Off-task unique rates.
            OutputPerKill(31996, "Dragon metal sheet", 1m / 100m),
            OutputPerKill(31406, "Dragon nails", 3m / 13m),

            // Equipment.
            OutputPerKill(1319, "Rune 2h sword", 1m / 65m),
            OutputPerKill(1201, "Rune kiteshield", 1m / 65m),
            OutputPerKill(1303, "Rune longsword", 1m / 65m),
            OutputPerKill(1275, "Rune pickaxe", 1m / 65m),
            OutputPerKill(1123, "Adamant platebody", 2m / 65m),
            OutputPerKill(1345, "Adamant warhammer", 2m / 65m),

            // Runes and ammunition. Range drops use their arithmetic mean quantity.
            OutputPerKill(560, "Death rune", 25m / 26m),
            OutputPerKill(555, "Water rune", 400m / 13m),
            OutputPerKill(556, "Air rune", 165m / 13m),
            OutputPerKill(565, "Blood rune", 15m / 26m),
            OutputPerKill(562, "Chaos rune", 56m / 13m),
            OutputPerKill(4695, "Mist rune", 50m / 13m),
            OutputPerKill(561, "Nature rune", 6m / 13m),
            OutputPerKill(31916, "Dragon cannonball", 16m / 65m),
            OutputPerKill(31914, "Rune cannonball", 11m / 13m),
            OutputPerKill(868, "Rune knife", 15m / 26m),

            // Resources and the tradeable tertiary drop.
            OutputPerKill(11237, "Dragon arrowtips", 3m / 13m),
            OutputPerKill(22124, "Superior dragon bones", 3m / 130m),
            OutputPerKill(451, "Runite ore", 3m / 130m),
            OutputPerKill(449, "Adamantite ore", 3m / 130m),
            OutputPerKill(2323, "Apple pie", 1m / 26m),
            OutputPerKill(11286, "Draconic visage", 1m / 10_000m)
        ]);

        return new(
            Slug,
            "Frost Dragons",
            pickUpFrostDragonBones
                ? "Off-task AFK melee with full Inquisitor armour, a dragon hunter lance on crush, an Avernic defender, Protect from Melee, Piety, and extended super antifire. Frost dragon bones are collected and banked. Rare and gem drop tables are excluded."
                : "Off-task AFK melee with full Inquisitor armour, a dragon hunter lance on crush, an Avernic defender, Protect from Melee, Piety, and extended super antifire. Frost dragon bones are left on the ground. Rare and gem drop tables are excluded.",
            DefaultKillsPerHour,
            Accounts: 1,
            GrandExchangeTaxRate: 0.02m,
            Items: items.ToArray());
    }

    private static ItemFlow InputPerHour(int itemId, string name, decimal quantity) =>
        new(itemId, name, quantity, ItemFlowDirection.Input);

    private static ItemFlow InputPerKill(int itemId, string name, decimal quantity) =>
        new(itemId, name, quantity, ItemFlowDirection.Input, QuantityBasis.PerAction);

    private static ItemFlow OutputPerKill(int itemId, string name, decimal quantity) =>
        new(itemId, name, quantity, ItemFlowDirection.Output, QuantityBasis.PerAction);
}
