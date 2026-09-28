using System.Collections.ObjectModel;
using System.Globalization;
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
    private readonly bool methodApplicable;

    public TrainingConfigurationOptionViewModel(
        TrainingConfigurationOption definition,
        string value,
        string? methodId)
    {
        this.definition = definition;
        IsToggle = definition.Kind == TrainingConfigurationOptionKind.Toggle;
        IsChoice = definition.Kind == TrainingConfigurationOptionKind.Choice;
        IsNumber = definition.Kind == TrainingConfigurationOptionKind.Number;
        methodApplicable = definition.AppliesTo(methodId);
        isApplicable = methodApplicable;
        Choices = new ObservableCollection<TrainingConfigurationChoiceViewModel>(
            (definition.Choices ?? [])
            .Select(choice => new TrainingConfigurationChoiceViewModel(choice)));
        toggleValue = bool.TryParse(value, out var enabled) && enabled;
        numberValue = IsNumber ? value : string.Empty;
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
    public bool IsNumber { get; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailabilityMessage))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    private bool isApplicable;
    public string AvailabilityMessage => IsApplicable
        ? string.Empty
        : methodApplicable && definition.RequiredToggleKey is not null
            ? "Enable the prerequisite option first."
            : "Not available for the selected method.";

    public void UpdateAvailability(IReadOnlyDictionary<string, string> values)
    {
        var prerequisiteEnabled = definition.RequiredToggleKey is null
            || (values.TryGetValue(definition.RequiredToggleKey, out var value)
                && bool.TryParse(value, out var enabled) && enabled);
        IsApplicable = methodApplicable && prerequisiteEnabled;
        if (!prerequisiteEnabled && IsToggle)
            ToggleValue = false;
    }
    public ObservableCollection<TrainingConfigurationChoiceViewModel> Choices { get; }

    [ObservableProperty]
    private bool toggleValue;

    [ObservableProperty]
    private TrainingConfigurationChoiceViewModel? selectedChoice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyPropertyChangedFor(nameof(ValidationMessage))]
    private string numberValue = string.Empty;

    public bool IsValid => !IsApplicable || !IsNumber || ValidateNumber() is null;
    public string ValidationMessage => ValidateNumber() ?? string.Empty;

    public string GetValue()
    {
        if (IsToggle)
            return ToggleValue ? bool.TrueString : bool.FalseString;
        if (IsChoice)
            return SelectedChoice?.Value ?? definition.DefaultValue;
        return NumberValue;
    }

    public void Reset()
    {
        if (IsToggle)
        {
            ToggleValue =
                bool.TryParse(definition.DefaultValue, out var enabled)
                && enabled;
            return;
        }

        if (IsNumber)
        {
            NumberValue = definition.DefaultValue;
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

    private string? ValidateNumber()
    {
        if (!IsNumber || !IsApplicable)
            return null;
        if (string.IsNullOrWhiteSpace(NumberValue))
            return definition.AllowsEmpty ? null : "Enter a value.";
        if (!TryParseNumber(NumberValue, out var number))
            return "Enter a valid number.";
        if (definition.WholeNumbersOnly && number != decimal.Truncate(number))
            return "Enter a whole number.";
        if (definition.MinimumValue.HasValue && number < definition.MinimumValue.Value)
            return $"Minimum: {definition.MinimumValue.Value:N0}.";
        if (definition.MaximumValue.HasValue && number > definition.MaximumValue.Value)
            return $"Maximum: {definition.MaximumValue.Value:N0}.";
        return null;
    }

    private static bool TryParseNumber(string value, out decimal number) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out number)
        || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out number);
}

public partial class TrainingConfigurationDialogViewModel : ObservableObject
{
    private readonly TrainingConfigurationDefinition definition;
    private bool updatingAvailability;

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
        foreach (var option in Options)
        {
            option.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(TrainingConfigurationOptionViewModel.IsValid))
                    OnPropertyChanged(nameof(IsValid));
                if (args.PropertyName == nameof(TrainingConfigurationOptionViewModel.ToggleValue))
                    UpdateAvailability();
            };
        }
        UpdateAvailability();
    }

    public string Skill { get; }
    public string Method { get; }
    public string Title => $"{Skill} configuration";
    public bool HasOptions => Options.Count > 0;
    public bool IsValid => Options.All(option => option.IsValid);
    public ObservableCollection<TrainingConfigurationOptionViewModel> Options { get; }

    public Dictionary<string, string> ToValues() =>
        definition.Normalize(
            Options.ToDictionary(
                option => option.Key,
                option => option.GetValue(),
                StringComparer.OrdinalIgnoreCase))
        .ToDictionary();

    private void UpdateAvailability()
    {
        if (updatingAvailability)
            return;
        updatingAvailability = true;
        try
        {
            var values = ToValues();
            foreach (var option in Options)
                option.UpdateAvailability(values);
        }
        finally
        {
            updatingAvailability = false;
        }
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        updatingAvailability = true;
        try
        {
            foreach (var option in Options)
                option.Reset();
        }
        finally
        {
            updatingAvailability = false;
        }
        UpdateAvailability();
    }
}
