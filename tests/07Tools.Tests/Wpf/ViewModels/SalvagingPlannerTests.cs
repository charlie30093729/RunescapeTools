namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class SalvagingPlannerTests
{
    private const decimal Bonus = 250m * 3600m / 63m;
    private static XpPlannerRowViewModel Create(TrainingSkillPreference? preference = null) => new(
        new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Sailing"),
        new TrainingPlanCalculator(), 13034431, preference, new Dictionary<int, ItemPrice>(), () => { });

    [Test]
    public void CustomBaseSurvivesTogglesRestoreAndReset()
    {
        var row = Create();
        row.SelectedMethodOption = row.MethodOptions.Single(option => option.Id == "salvaging");
        Assert.That(row.PersonalRate, Is.EqualTo(95000m + Bonus));
        row.PersonalRate = 90000m;
        Assert.That(row.PersonalRate, Is.EqualTo(90000m + Bonus));
        for (var i = 0; i < 3; i++)
        {
            row.ApplyConfiguration(new Dictionary<string, string> { ["crystal-extractor"] = "False" });
            Assert.That(row.PersonalRate, Is.EqualTo(90000m));
            row.ApplyConfiguration(new Dictionary<string, string> { ["crystal-extractor"] = "True" });
            Assert.That(row.PersonalRate, Is.EqualTo(90000m + Bonus));
        }
        var preference = row.ToPreference();
        Assert.That(preference.ExperiencePerHourOverride, Is.EqualTo(90000m));
        var restored = Create(preference);
        Assert.That(restored.PersonalRate, Is.EqualTo(row.PersonalRate));
        Assert.That(restored.Result.Hours, Is.EqualTo(row.Result.Hours));
        restored.ResetSkillCommand.Execute(null);
        Assert.That(restored.SelectedMethodOption?.Id, Is.EqualTo("salvaging"));
        Assert.That(restored.PersonalRate, Is.EqualTo(95000m + Bonus));
        Assert.That(restored.ToPreference().ExperiencePerHourOverride, Is.Null);
    }

    [Test]
    public void StableLabelAndUnlockRefreshWhenStartXpChanges()
    {
        var row = Create();
        row.SelectedMethodOption = row.MethodOptions.Single(option => option.Id == "salvaging");
        foreach (var start in new long[] { 200000000, 3972294, 992895, 2411 })
        {
            row.StartExperience = start;
            Assert.That(row.SelectedMethodOption.Name, Is.EqualTo("Salvaging"));
        }
        row.StartExperience = 0;
        Assert.That(row.SelectedMethodOption.Name, Is.EqualTo("Salvaging — unlocks at 15"));
        Assert.That(row.HasAvailabilitySummary, Is.True);
        row.StartExperience = 3972294;
        Assert.That(row.PersonalRate, Is.EqualTo(95000m + Bonus));
        Assert.That(row.HasAvailabilitySummary, Is.False);
    }
}
