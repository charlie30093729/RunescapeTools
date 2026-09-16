namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class CalcifiedRocksPlannerTests
{
    private static XpPlannerRowViewModel Create(TrainingSkillPreference? preference = null) => new(
        new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Mining"),
        new TrainingPlanCalculator(), 13034431, preference,
        new Dictionary<int, ItemPrice> { [23959] = Quote(23959, 3000000) }, () => { });

    [Test]
    public void SelectionRateOverrideAndResetRoundTripThroughPersistence()
    {
        var row = Create();
        row.SelectedMethodOption = row.MethodOptions.Single(option => option.Id == "calcified-rocks-crystal-pickaxe");
        Assert.That(row.PersonalRate, Is.EqualTo(50225m));
        row.PersonalRate = 45000m;
        row.TargetExperience = 50000000;
        Assert.That(row.HasConfiguration, Is.True);
        row.ApplyConfiguration(new Dictionary<string, string> { ["prospector-outfit"] = "False" });
        Assert.That(row.PersonalRate, Is.EqualTo(45000m));
        row.ApplyConfiguration(new Dictionary<string, string> { ["prospector-outfit"] = "True" });
        Assert.That(row.PersonalRate, Is.EqualTo(46125m));
        var restored = Create(row.ToPreference());
        Assert.That(restored.SelectedMethodOption?.Id, Is.EqualTo("calcified-rocks-crystal-pickaxe"));
        Assert.That(restored.PersonalRate, Is.EqualTo(46125m));
        Assert.That(restored.Result.Hours, Is.EqualTo(row.Result.Hours));
        restored.ResetSkillCommand.Execute(null);
        Assert.That(restored.PersonalRate, Is.EqualTo(50225m));
        Assert.That(restored.ConfigurationValues["prospector-outfit"], Is.EqualTo(bool.TrueString));
        Assert.That(restored.ToPreference().ExperiencePerHourOverride, Is.Null);
    }

    [Test]
    public async Task PreferencePersistsAndUntradeableRewardsDisplayAsGains()
    {
        var row = Create();
        row.SelectedMethodOption = row.MethodOptions.Single(option => option.Id == "calcified-rocks-crystal-pickaxe");
        row.PersonalRate = 45000m;
        using var temporary = new TemporaryDirectory();
        var store = new JsonTrainingPlanStore(new TrainingPlanOptions
        {
            FilePath = Path.Combine(temporary.Path, "training-plans.json")
        });
        await store.SaveAsync("Miner", [row.ToPreference()]);
        var restored = Create((await store.GetAsync("Miner"))["Mining"]);
        Assert.That(restored.PersonalRate, Is.EqualTo(46125m));
        Assert.That(restored.SelectedMethodOption?.Id, Is.EqualTo("calcified-rocks-crystal-pickaxe"));
        var dialog = new TrainingPriceDialogViewModel("Mining", restored.Result,
            new Dictionary<int, ItemPrice> { [23959] = Quote(23959, 3000000) });
        Assert.That(dialog.Items.Single(item => item.ItemId == 23959).Action, Is.EqualTo("BUY"));
        foreach (var item in dialog.Items.Where(item => item.ItemId is 29381 or 29088))
        {
            Assert.That(item.Action, Is.EqualTo("GAIN"));
            Assert.That(item.UnitPrice, Is.EqualTo("Untradeable"));
        }
    }

    [Test]
    public void LabelsAndRatesRefreshWhenMovingFrom200mBelowUnlockAndBack()
    {
        var row = Create();
        row.SelectedMethodOption = row.MethodOptions.Single(option => option.Id == "calcified-rocks-crystal-pickaxe");
        row.StartExperience = 200000000;
        Assert.That(row.SelectedMethodOption?.Name, Is.EqualTo("Calcified rocks - crystal pickaxe"));
        row.StartExperience = 814444;
        Assert.That(row.SelectedMethodOption?.Name, Is.EqualTo("Calcified rocks - crystal pickaxe — unlocks at 71"));
        Assert.That(row.HasAvailabilitySummary, Is.True);
        row.StartExperience = 814445;
        Assert.That(row.SelectedMethodOption?.Name, Is.EqualTo("Calcified rocks - crystal pickaxe"));
        Assert.That(row.PersonalRate, Is.EqualTo(34850m));
        Assert.That(row.HasAvailabilitySummary, Is.False);
        row.StartExperience = 13034431;
        Assert.That(row.PersonalRate, Is.EqualTo(50225m));
    }
}
