using System.Collections.ObjectModel;
using RunescapeTools.Core.Market;
using RunescapeTools.Core.Training;

namespace RunescapeTools.Wpf.ViewModels;

public sealed record TrainingPriceItemRowViewModel(
    string Action,
    string Name,
    string ItemNumber,
    string Quantity,
    string QuantityCaption,
    string UnitPrice,
    string QuoteDetail,
    bool IsOutput,
    bool HasPrice);

public sealed class TrainingPriceDialogViewModel
{
    public TrainingPriceDialogViewModel(
        string skill,
        TrainingSkillPlanResult result,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
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
        PricingStatus = result.IsFullyPriced
            ? "Fully priced"
            : result.PricedExperience > 0
                ? $"{(decimal)result.PricedExperience / Math.Max(1, result.ExperienceRemaining):P2} priced"
                : "Rate only";

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
    public string PricingStatus { get; }
    public bool HasItems => Items.Count > 0;
    public ObservableCollection<TrainingPriceItemRowViewModel> Items { get; }

    private static TrainingPriceItemRowViewModel CreateItemRow(
        TrainingResourceRequirement requirement,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
        prices.TryGetValue(requirement.ItemId, out var quote);
        var selected = TrainingMarketPricing.Select(requirement.Direction, quote);
        var isOutput = requirement.Direction == TrainingFlowDirection.Output;
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
