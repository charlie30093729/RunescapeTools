using RunescapeTools.Core.MoneyMaking;

namespace RunescapeTools.Wpf.ViewModels;

public sealed record SelectedMoneyMaker(
    string Slug,
    string Name,
    decimal ProfitPerAccountPerHour,
    int AccountCount,
    bool HasMissingPrices,
    MoneyMakingMethodDefinition? Definition = null)
{
    public decimal TotalProfitPerHour => ProfitPerAccountPerHour * AccountCount;
}

public sealed class MoneyMakerSelectionContext
{
    public event EventHandler? SelectionChanged;

    public SelectedMoneyMaker? Current { get; private set; }

    public void Select(
        string slug,
        string name,
        decimal profitPerAccountPerHour,
        int accountCount,
        bool hasMissingPrices,
        MoneyMakingMethodDefinition? definition = null)
    {
        if (accountCount < 1)
            throw new ArgumentOutOfRangeException(nameof(accountCount), "Account count must be at least one.");

        var next = new SelectedMoneyMaker(
            slug,
            name,
            profitPerAccountPerHour,
            accountCount,
            hasMissingPrices,
            definition);
        if (next == Current)
            return;

        Current = next;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        if (Current is null)
            return;

        Current = null;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
