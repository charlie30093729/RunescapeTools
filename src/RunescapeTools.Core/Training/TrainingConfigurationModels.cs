namespace RunescapeTools.Core.Training;

public enum TrainingConfigurationOptionKind
{
    Toggle,
    Choice
}

public sealed record TrainingConfigurationChoice(
    string Value,
    string Label,
    bool IsEnabled = true,
    string? Description = null);

public sealed record TrainingConfigurationOption(
    string Key,
    string Label,
    TrainingConfigurationOptionKind Kind,
    string DefaultValue,
    string? Description = null,
    IReadOnlyList<TrainingConfigurationChoice>? Choices = null,
    IReadOnlyList<string>? ApplicableMethodIds = null)
{
    public bool AppliesTo(string? methodId) =>
        ApplicableMethodIds is not { Count: > 0 }
        || ApplicableMethodIds.Any(id =>
            string.Equals(id, methodId, StringComparison.OrdinalIgnoreCase));
}

public sealed class TrainingConfigurationDefinition(
    IReadOnlyList<TrainingConfigurationOption> options)
{
    public IReadOnlyList<TrainingConfigurationOption> Options { get; } = options;

    public TrainingConfigurationValues Normalize(
        IReadOnlyDictionary<string, string>? values = null)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var option in Options)
        {
            string? supplied = null;
            values?.TryGetValue(option.Key, out supplied);
            normalized[option.Key] = Normalize(option, supplied);
        }

        return new TrainingConfigurationValues(normalized);
    }

    private static string Normalize(TrainingConfigurationOption option, string? value)
    {
        if (option.Kind == TrainingConfigurationOptionKind.Toggle)
        {
            if (bool.TryParse(value, out var enabled))
                return enabled ? bool.TrueString : bool.FalseString;
            return bool.TryParse(option.DefaultValue, out var defaultEnabled) && defaultEnabled
                ? bool.TrueString
                : bool.FalseString;
        }

        var selected = option.Choices?.FirstOrDefault(choice =>
            choice.IsEnabled
            && string.Equals(choice.Value, value, StringComparison.OrdinalIgnoreCase));
        if (selected is not null)
            return selected.Value;

        return option.Choices?.FirstOrDefault(choice =>
                   choice.IsEnabled
                   && string.Equals(
                       choice.Value,
                       option.DefaultValue,
                       StringComparison.OrdinalIgnoreCase))?.Value
               ?? option.Choices?.FirstOrDefault(choice => choice.IsEnabled)?.Value
               ?? option.DefaultValue;
    }
}

public sealed class TrainingConfigurationValues(
    IReadOnlyDictionary<string, string> values)
{
    public static TrainingConfigurationValues Empty { get; } =
        new(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

    public IReadOnlyDictionary<string, string> Values { get; } = values;

    public bool GetToggle(string key)
    {
        return Values.TryGetValue(key, out var value)
               && bool.TryParse(value, out var enabled)
               && enabled;
    }

    public string GetChoice(string key)
    {
        return Values.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public Dictionary<string, string> ToDictionary() =>
        new(Values, StringComparer.OrdinalIgnoreCase);
}

public interface ITrainingSkillConfigurator
{
    TrainingConfigurationDefinition Definition { get; }

    TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method,
        TrainingConfigurationValues configuration);

    bool IncludeHours(
        TrainingMethodDefinition method,
        TrainingConfigurationValues configuration);
}
