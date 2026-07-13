using System.Windows;
using RunescapePriceChecker.Wpf.ViewModels;

namespace RunescapePriceChecker.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
