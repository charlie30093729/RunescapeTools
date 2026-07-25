using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training;

internal static class TrainingCatalogueBuilder
{
    public static TrainingSkillDefinition Skill(
        string name,
        TrainingRateBand band,
        string? note = null) =>
        new(name, [band], Note: note);

    public static TrainingSkillDefinition Skill(
        string name,
        string note,
        params TrainingRateBand[] bands) =>
        new(name, bands, Note: note);

    public static TrainingSkillDefinition Skill(string name, params TrainingRateBand[] bands) =>
        new(name, bands);

    public static TrainingRateBand Band(
        long startExperience,
        decimal experiencePerHour,
        string method,
        TrainingEconomics? economics = null) =>
        new(startExperience, experiencePerHour, method, economics);

    public static TrainingRateBand Standalone(string method, decimal rate) =>
        Band(0, rate, method);

    public static TrainingResourceFlow Input(
        int itemId,
        string name,
        decimal quantityPerExperience,
        decimal quantityPerHour = 0m) =>
        new(
            itemId,
            name,
            quantityPerExperience,
            TrainingFlowDirection.Input,
            QuantityPerHour: quantityPerHour);

    public static TrainingResourceFlow Output(
        int itemId,
        string name,
        decimal quantityPerExperience) =>
        new(itemId, name, quantityPerExperience, TrainingFlowDirection.Output);
}
