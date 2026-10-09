using static RunescapeTools.Tests.TestSupport.Builders.PrayerOfferingTestData;

namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
[Category("Persistence")]
public sealed class PrayerOfferingPreferenceTests
{
    [TestCase("offering-at-bank")]
    [TestCase("offering-at-prif-agility")]
    public async Task OfferingLocationAndBoneChoiceSurviveJsonRoundTrip(string location)
    {
        using var directory = new TemporaryDirectory();
        var options = new TrainingPlanOptions { FilePath = Path.Combine(directory.Path, "training.json") };
        var store = new JsonTrainingPlanStore(options);
        await store.SaveAsync("bottleo", [new TrainingSkillPreference("Prayer", 200_000_000,
            TrainingMethodId: "frost-dragon-bones", Configuration: Location(location))]);
        var loaded = await new JsonTrainingPlanStore(options).GetAsync("bottleo");
        Assert.That(loaded["Prayer"].TrainingMethodId, Is.EqualTo("frost-dragon-bones"));
        Assert.That(loaded["Prayer"].Configuration!["offering-location"], Is.EqualTo(location));
    }
}
