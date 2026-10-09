namespace RunescapeTools.Core.MoneyMaking.Methods;

/// <summary>
/// Efficient, low-intensity rune-dragon trips using the documented 45-kill baseline.
/// Supplies and expected drops are expressed per kill so a personal kill-rate override
/// scales the complete ledger rather than changing loot alone.
/// </summary>
public sealed class RuneDragonMethod : IMoneyMakingMethod
{
    private const decimal DefaultKillsPerHour = 45m;

    public MoneyMakingMethodDefinition Definition { get; } = new(
        Slug: "rune-dragons",
        Name: "Rune Dragons",
        Description:
            "Efficient low-intensity melee using Justiciar, a dragon hunter lance, dragonfire shield, Piety, Protect from Magic, and a mounted Digsite pendant. Assumes nine kills per trip, Monkey Madness II drops, and excludes the small variable rare-drop-table value.",
        ActionsPerHour: DefaultKillsPerHour,
        Accounts: 1,
        GrandExchangeTaxRate: 0.02m,
        Items:
        [
            // Supplies: the reviewed guide's hourly quantities at 45 kills per hour.
            Input(2434, "Prayer potion(4)", 15m),
            Input(23685, "Divine super combat potion(4)", 2.5m),
            Input(11951, "Extended antifire(4)", 1.25m),
            Input(3144, "Cooked karambwan", 100m),
            Input(8013, "Teleport to house (tablet)", 5m),

            // Guaranteed drops.
            Output(2363, "Runite bar", 45m),
            Output(536, "Dragon bones", 45m),

            // Expected uniques, equipment, ammunition, herbs, and resources.
            Output(11286, "Draconic visage", 0.0056m),
            Output(22103, "Dragon metal lump", 0.009m),
            Output(21918, "Dragon limbs", 0.056m),
            Output(1127, "Rune platebody", 3.19m),
            Output(1303, "Rune longsword", 2.83m),
            Output(1432, "Rune mace", 2.48m),
            Output(1333, "Rune scimitar", 2.48m),
            Output(1347, "Rune warhammer", 2.48m),
            Output(1079, "Rune platelegs", 2.13m),
            Output(4087, "Dragon platelegs", 0.35m),
            Output(4585, "Dragon plateskirt", 0.35m),
            Output(1149, "Dragon med helm", 0.35m),
            Output(21880, "Wrath rune", 113m),
            Output(562, "Chaos rune", 279m),
            Output(560, "Death rune", 186m),
            Output(9381, "Runite bolts (unf)", 97.44m),
            Output(19580, "Rune javelin tips", 88.58m),
            Output(892, "Rune arrow", 99.21m),
            Output(19582, "Dragon javelin tips", 62.01m),
            Output(21930, "Dragon bolts (unf)", 10.63m),
            Output(211, "Grimy avantoe", 0.89m),
            Output(207, "Grimy ranarr weed", 0.71m),
            Output(3051, "Grimy snapdragon", 0.71m),
            Output(219, "Grimy torstol", 0.53m),
            Output(1615, "Dragonstone", 2.48m),
            Output(451, "Runite ore", 7.44m),
            Output(22118, "Wrath talisman", 0.35m)
        ]);

    private static ItemFlow Input(int itemId, string name, decimal quantityPerDefaultHour) =>
        PerKill(itemId, name, quantityPerDefaultHour, ItemFlowDirection.Input);

    private static ItemFlow Output(int itemId, string name, decimal quantityPerDefaultHour) =>
        PerKill(itemId, name, quantityPerDefaultHour, ItemFlowDirection.Output);

    private static ItemFlow PerKill(
        int itemId,
        string name,
        decimal quantityPerDefaultHour,
        ItemFlowDirection direction) =>
        new(
            itemId,
            name,
            quantityPerDefaultHour / DefaultKillsPerHour,
            direction,
            QuantityBasis.PerAction);
}
