namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
[Category("Persistence")]
public sealed class JsonTrainingPlanStoreTests
{
    [Test]
    [Property("LegacyScenario", "TrainingPlanPersistence")]
    [Description("training plans persist independently per RSN")]
    public async Task TrainingPlanPersistence()
    {
        using var temporary = new TemporaryDirectory();
        var directory = temporary.Path;
        var path = Path.Combine(directory, "training-plans.json");
        var store = new JsonTrainingPlanStore(new TrainingPlanOptions { FilePath = path });
        await store.SaveAsync(
            "Player One",
            [new TrainingSkillPreference(
                "Construction",
                200_000_000,
                0,
                1_070_000,
                true,
                "mahogany-benches",
                new Dictionary<string, string>
                {
                    ["carpenters-outfit"] = bool.TrueString
                })]);
        await store.SaveAsync("Player Two", [new TrainingSkillPreference("Construction", 13_034_431)]);

        var first = await store.GetAsync(" player one ");
        var second = await store.GetAsync("PLAYER TWO");
        Assert.That(first["Construction"].TargetExperience, Is.EqualTo(200_000_000L), "first profile goal");
        Assert.That(second["Construction"].TargetExperience, Is.EqualTo(13_034_431L), "second profile goal");
        Assert.That(first["Construction"].StartExperienceOverride == 0, Is.True, "explicit zero-XP override persists");
        Assert.That(first["Construction"].IsMoneyMakingSelected, Is.True, "money-making skill allocation persists");
        Assert.That(first["Construction"].TrainingMethodId ?? string.Empty, Is.EqualTo("mahogany-benches"), "training method selection persists");
        Assert.That(first["Construction"].Configuration!["carpenters-outfit"], Is.EqualTo(bool.TrueString), "training configuration persists");
    }
}
