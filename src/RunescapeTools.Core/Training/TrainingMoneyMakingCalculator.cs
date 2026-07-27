namespace RunescapeTools.Core.Training;

public sealed record TrainingMoneyMakingResult(
    decimal SelectedHours,
    decimal NetGp);

public sealed class TrainingMoneyMakingCalculator
{
    public TrainingMoneyMakingResult Calculate(
        decimal? profitPerAccountPerHour,
        IEnumerable<decimal> selectedSkillHours)
    {
        ArgumentNullException.ThrowIfNull(selectedSkillHours);

        var hours = selectedSkillHours.Sum(value => Math.Max(0m, value));
        var netGp = hours * (profitPerAccountPerHour ?? 0m);
        return new TrainingMoneyMakingResult(hours, netGp);
    }
}
