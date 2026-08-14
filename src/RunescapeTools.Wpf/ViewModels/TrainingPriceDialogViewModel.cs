using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.Training;

namespace RunescapeTools.Wpf.ViewModels;

public partial class TrainingPriceItemRowViewModel(
    int itemId,
    string action,
    string name,
    string itemNumber,
    string quantity,
    string quantityCaption,
    string unitPrice,
    string quoteDetail,
    bool isOutput,
    bool hasPrice,
    bool isSupplied = false) : ObservableObject
{
    public int ItemId { get; } = itemId;
    public string Action { get; } = action;
    public string Name { get; } = name;
    public string ItemNumber { get; } = itemNumber;
    public string Quantity { get; } = quantity;
    public string QuantityCaption { get; } = quantityCaption;
    public string UnitPrice { get; } = unitPrice;
    public string QuoteDetail { get; } = quoteDetail;
    public bool IsOutput { get; } = isOutput;
    public bool HasPrice { get; } = hasPrice;
    public bool IsSupplied { get; } = isSupplied;

    [ObservableProperty]
    private string? iconPath;
}

public sealed class TrainingPriceDialogViewModel
{
    private readonly IItemIconService? itemIcons;

    public TrainingPriceDialogViewModel(
        string skill,
        TrainingSkillPlanResult result,
        IReadOnlyDictionary<int, ItemPrice> prices,
        IItemIconService? itemIcons = null)
    {
        this.itemIcons = itemIcons;
        Skill = skill;
        Method = result.Method.Name;
        GoalSummary =
            $"{result.EffectiveStartExperience:N0} XP to {result.TargetExperience:N0} XP"
            + $" - {result.ExperienceRemaining:N0} XP remaining";
        RouteSummary = string.Join(
            " -> ",
            result.Bands
                .Select(band => band.Band.Method)
                .Distinct(StringComparer.OrdinalIgnoreCase));
        EstimatedNetGp = result.NetGp.HasValue
            ? DisplayFormat.Gp(result.NetGp)
            : "Not priced";
        GpPerExperience = DisplayFormat.GpPerExperience(result.GpPerExperience);

        Items = new ObservableCollection<TrainingPriceItemRowViewModel>(
            result.ResourceRequirements.Select(requirement =>
                CreateItemRow(requirement, prices)));
    }

    public string Skill { get; }
    public string Title => $"{Skill} item recommendations";
    public string Method { get; }
    public string GoalSummary { get; }
    public string RouteSummary { get; }
    public string EstimatedNetGp { get; }
    public string GpPerExperience { get; }
    public bool HasItems => Items.Count > 0;
    public ObservableCollection<TrainingPriceItemRowViewModel> Items { get; }

    public async Task LoadIconsAsync(CancellationToken cancellationToken = default)
    {
        if (itemIcons is null || Items.Count == 0)
            return;

        var icons = await itemIcons.GetManyAsync(
            Items.Select(item => item.ItemId),
            cancellationToken);
        foreach (var item in Items)
        {
            if (icons.TryGetValue(item.ItemId, out var icon))
                item.IconPath = icon.LocalFilePath;
        }
    }

    private static TrainingPriceItemRowViewModel CreateItemRow(
        TrainingResourceRequirement requirement,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
        var isOutput = requirement.Direction == TrainingFlowDirection.Output;
        if (!requirement.RequiresMarketPrice)
        {
            return new TrainingPriceItemRowViewModel(
                requirement.ItemId,
                "USE",
                requirement.Name,
                $"Item {requirement.ItemId}",
                Math.Ceiling(requirement.Quantity).ToString("N0"),
                "required from your stock",
                "Untradeable",
                "No Grand Exchange price is applied",
                isOutput: false,
                hasPrice: true,
                isSupplied: true);
        }

        prices.TryGetValue(requirement.ItemId, out var quote);
        var selected = TrainingMarketPricing.Select(requirement.Direction, quote);
        var preferredSide = isOutput ? "low" : "high";
        var fallbackSide = isOutput ? "high" : "low";
        var unitPrice = selected.UnitPrice.HasValue
            ? $"{selected.UnitPrice.Value:N0} gp"
            : "Unavailable";
        var quoteDetail = !selected.UnitPrice.HasValue
            ? "No high or low quote available"
            : $"{(selected.UsedFallbackPrice ? fallbackSide + " fallback" : preferredSide)}"
              + (selected.Timestamp.HasValue
                  ? $" - {selected.Timestamp.Value.ToUniversalTime():yyyy-MM-dd HH:mm} UTC"
                  : " - timestamp unavailable");

        return new TrainingPriceItemRowViewModel(
            requirement.ItemId,
            isOutput ? "SELL" : "BUY",
            requirement.Name,
            $"Item {requirement.ItemId}",
            isOutput
                ? $"~ {DisplayFormat.Quantity(requirement.Quantity)}"
                : Math.Ceiling(requirement.Quantity).ToString("N0"),
            isOutput ? "expected output" : "required for goal",
            unitPrice,
            quoteDetail,
            isOutput,
            selected.UnitPrice.HasValue);
    }
}
