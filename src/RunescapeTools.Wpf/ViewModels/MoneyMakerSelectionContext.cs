namespace RunescapeTools.Wpf.ViewModels;

public sealed record SelectedMoneyMaker(
    string Slug,
    string Name,
    decimal ProfitPerAccountPerHour,
    bool HasMissingPrices);

public sealed class MoneyMakerSelectionContext
{
    public event EventHandler? SelectionChanged;

    public SelectedMoneyMaker? Current { get; private set; }

    public void Select(
        string slug,
        string name,
        decimal profitPerAccountPerHour,
        bool hasMissingPrices)
    {
        var next = new SelectedMoneyMaker(
            slug,
            name,
            profitPerAccountPerHour,
            hasMissingPrices);
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
