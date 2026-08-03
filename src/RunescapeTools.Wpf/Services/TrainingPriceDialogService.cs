using RunescapeTools.Core.Market;
using RunescapeTools.Core.Training;
using RunescapeTools.Application.Market;
using RunescapeTools.Wpf.ViewModels;
using RunescapeTools.Wpf.Views;

namespace RunescapeTools.Wpf.Services;

public sealed class TrainingPriceDialogService(IItemIconService itemIcons) : ITrainingPriceDialogService
{
    public void Show(
        string skill,
        TrainingSkillPlanResult result,
        IReadOnlyDictionary<int, ItemPrice> prices)
    {
        var dialog = new TrainingPriceDialog(
            new TrainingPriceDialogViewModel(skill, result, prices, itemIcons))
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        dialog.ShowDialog();
    }
}
