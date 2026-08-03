using System.Windows;
using RunescapeTools.Wpf.ViewModels;

namespace RunescapeTools.Wpf.Views;

public partial class TrainingPriceDialog : Window
{
    private readonly TrainingPriceDialogViewModel viewModel;
    private readonly CancellationTokenSource closing = new();

    public TrainingPriceDialog(TrainingPriceDialogViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = viewModel;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        try
        {
            await viewModel.LoadIconsAsync(closing.Token);
        }
        catch (OperationCanceledException) when (closing.IsCancellationRequested)
        {
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        closing.Cancel();
        closing.Dispose();
        base.OnClosed(e);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
