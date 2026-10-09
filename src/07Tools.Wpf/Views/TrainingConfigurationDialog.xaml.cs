using System.Windows;
using RunescapeTools.Wpf.ViewModels;

namespace RunescapeTools.Wpf.Views;

public partial class TrainingConfigurationDialog : Window
{
    public TrainingConfigurationDialog(
        TrainingConfigurationDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Apply_Click(object sender, RoutedEventArgs e) =>
        DialogResult = true;

    private void Cancel_Click(object sender, RoutedEventArgs e) =>
        DialogResult = false;
}
