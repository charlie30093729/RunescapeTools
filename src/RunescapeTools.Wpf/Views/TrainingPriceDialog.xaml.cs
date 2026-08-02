using System.Windows;
using RunescapeTools.Wpf.ViewModels;

namespace RunescapeTools.Wpf.Views;

public partial class TrainingPriceDialog : Window
{
    public TrainingPriceDialog(TrainingPriceDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
