using RunescapeTools.Core.Training;

namespace RunescapeTools.Wpf.Services;

public interface ITrainingConfigurationDialogService
{
    IReadOnlyDictionary<string, string>? Edit(
        string skill,
        string method,
        string? methodId,
        TrainingConfigurationDefinition definition,
        IReadOnlyDictionary<string, string> currentValues);
}
