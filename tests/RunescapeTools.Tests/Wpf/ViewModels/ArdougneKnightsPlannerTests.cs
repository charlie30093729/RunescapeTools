namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class ArdougneKnightsPlannerTests
{
    [Test]
    public void SelectionUnlockRatePersistenceAndResetUseArdougneKnights()
    {
        var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Thieving");
        var calculator = new TrainingPlanCalculator();
        var prices = new Dictionary<int, ItemPrice> { [6571] = Quote(6571, 2_600_000) };
        var row = new XpPlannerRowViewModel(definition, calculator, 166_635, null, prices, () => { });
        var option = row.MethodOptions.Single(method => method.Id == "ardougne-knights");

        Assert.That(row.SelectedMethodOption!.Id, Is.EqualTo("main-ehp"));
        Assert.That(option.Name, Is.EqualTo("Ardougne knights — unlocks at 55"));
        row.SelectedMethodOption = option;
        Assert.That(row.Method, Is.EqualTo("Gem knights"));
        Assert.That(row.HasAvailabilitySummary, Is.True);

        row.StartExperience = 166_636;
        Assert.That(option.Name, Is.EqualTo("Ardougne knights"));
        Assert.That(row.Method, Is.EqualTo("Ardougne knights"));
        Assert.That(row.PersonalRate, Is.EqualTo(82_369m));
        Assert.That(row.HasAvailabilitySummary, Is.False);
        row.StartExperience = 13_034_431;
        Assert.That(row.PersonalRate, Is.EqualTo(252_900m));
        row.PersonalRate = 200_000m;
        var restored = new XpPlannerRowViewModel(definition, calculator, 166_635,
            row.ToPreference(), prices, () => { });
        Assert.That(restored.SelectedMethodOption!.Id, Is.EqualTo("ardougne-knights"));
        Assert.That(restored.PersonalRate, Is.EqualTo(200_000m));
        Assert.That(restored.Result.NetGp, Is.EqualTo(row.Result.NetGp));

        row.ResetSkillCommand.Execute(null);
        Assert.That(row.SelectedMethodOption!.Id, Is.EqualTo("ardougne-knights"));
        Assert.That(row.StartExperience, Is.EqualTo(166_635));
        Assert.That(row.PersonalRate, Is.EqualTo(260_000m));
    }
}
