using RunescapeTools.Core.Market;
using RunescapeTools.Core.Training;

namespace RunescapeTools.Wpf.Services;

public interface ITrainingPriceDialogService
{
    void Show(
        string skill,
        TrainingSkillPlanResult result,
        IReadOnlyDictionary<int, ItemPrice> prices);
}
