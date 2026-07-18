using System.Windows;
using RunescapeTools.Wpf.ViewModels;

namespace RunescapeTools.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
