using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunescapeTools.Core.Training;

namespace RunescapeTools.Wpf.ViewModels;

public sealed class TrainingConfigurationChoiceViewModel(
    TrainingConfigurationChoice choice)
{
    public string Value => choice.Value;
    public string Label => choice.Label;
    public bool IsEnabled => choice.IsEnabled;
    public string? Description => choice.Description;
    public string DisplayName => string.IsNullOrWhiteSpace(Description)
        ? Label
        : $"{Label} — {Description}";
}

public partial class TrainingConfigurationOptionViewModel : ObservableObject
{
    private readonly TrainingConfigurationOption definition;

    public TrainingConfigurationOptionViewModel(
        TrainingConfigurationOption definition,
        string value,
        string? methodId)
    {
        this.definition = definition;
        IsToggle = definition.Kind == TrainingConfigurationOptionKind.Toggle;
        IsChoice = definition.Kind == TrainingConfigurationOptionKind.Choice;
        IsApplicable = definition.AppliesTo(methodId);
        Choices = new ObservableCollection<TrainingConfigurationChoiceViewModel>(
            (definition.Choices ?? [])
            .Select(choice => new TrainingConfigurationChoiceViewModel(choice)));
        toggleValue = bool.TryParse(value, out var enabled) && enabled;
        selectedChoice = Choices.FirstOrDefault(choice =>
                             string.Equals(
                                 choice.Value,
                                 value,
                                 StringComparison.OrdinalIgnoreCase))
                         ?? Choices.FirstOrDefault(choice => choice.IsEnabled);
    }

    public string Key => definition.Key;
    public string Label => definition.Label;
    public string? Description => definition.Description;
    public bool IsToggle { get; }
    public bool IsChoice { get; }
    public bool IsApplicable { get; }
    public string AvailabilityMessage => IsApplicable
        ? string.Empty
        : "Not available for the selected method.";
    public ObservableCollection<TrainingConfigurationChoiceViewModel> Choices { get; }

    [ObservableProperty]
    private bool toggleValue;

    [ObservableProperty]
    private TrainingConfigurationChoiceViewModel? selectedChoice;

    public string GetValue() =>
        IsToggle
            ? ToggleValue ? bool.TrueString : bool.FalseString
            : SelectedChoice?.Value ?? definition.DefaultValue;

    public void Reset()
    {
        if (IsToggle)
        {
            ToggleValue =
                bool.TryParse(definition.DefaultValue, out var enabled)
                && enabled;
            return;
        }

        SelectedChoice = Choices.FirstOrDefault(choice =>
                             choice.IsEnabled
                             && string.Equals(
                                 choice.Value,
                                 definition.DefaultValue,
                                 StringComparison.OrdinalIgnoreCase))
                         ?? Choices.FirstOrDefault(choice => choice.IsEnabled);
    }
}

public partial class TrainingConfigurationDialogViewModel : ObservableObject
{
    private readonly TrainingConfigurationDefinition definition;

    public TrainingConfigurationDialogViewModel(
        string skill,
        string method,
        TrainingConfigurationDefinition definition,
        IReadOnlyDictionary<string, string> values,
        string? methodId)
    {
        Skill = skill;
        Method = method;
        this.definition = definition;
        var normalized = definition.Normalize(values);
        Options = new ObservableCollection<TrainingConfigurationOptionViewModel>(
            definition.Options.Select(option =>
                new TrainingConfigurationOptionViewModel(
                    option,
                    normalized.Values[option.Key],
                    methodId)));
    }

    public string Skill { get; }
    public string Method { get; }
    public string Title => $"{Skill} configuration";
    public bool HasOptions => Options.Count > 0;
    public ObservableCollection<TrainingConfigurationOptionViewModel> Options { get; }

    public Dictionary<string, string> ToValues() =>
        definition.Normalize(
            Options.ToDictionary(
                option => option.Key,
                option => option.GetValue(),
                StringComparer.OrdinalIgnoreCase))
        .ToDictionary();

    [RelayCommand]
    private void ResetDefaults()
    {
        foreach (var option in Options)
            option.Reset();
    }
}
