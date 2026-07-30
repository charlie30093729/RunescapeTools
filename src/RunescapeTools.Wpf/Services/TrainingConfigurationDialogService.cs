using RunescapeTools.Core.Training;
using RunescapeTools.Wpf.ViewModels;
using RunescapeTools.Wpf.Views;

namespace RunescapeTools.Wpf.Services;

public sealed class TrainingConfigurationDialogService
    : ITrainingConfigurationDialogService
{
    public IReadOnlyDictionary<string, string>? Edit(
        string skill,
        string method,
        string? methodId,
        TrainingConfigurationDefinition definition,
        IReadOnlyDictionary<string, string> currentValues)
    {
        var viewModel = new TrainingConfigurationDialogViewModel(
            skill,
            method,
            definition,
            currentValues,
            methodId);
        var dialog = new TrainingConfigurationDialog(viewModel)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        return dialog.ShowDialog() == true
            ? viewModel.ToValues()
            : null;
    }
}
