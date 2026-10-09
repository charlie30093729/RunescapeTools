namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
[Category("Persistence")]
public sealed class JsonProfilePreferenceStoreTests
{
    [Test]
    [Property("LegacyScenario", "ProfilePreferencePersistence")]
    [Description("profile preference seeds bottleo and persists successful selections")]
    public async Task ProfilePreferencePersistence()
    {
        using var temporary = new TemporaryDirectory();
        var directory = temporary.Path;
        var path = Path.Combine(directory, "profile.json");
        var store = new JsonProfilePreferenceStore(new ProfilePreferenceOptions
        {
            FilePath = path,
            DefaultRsn = "bottleo"
        });

        Assert.That(await store.GetSelectedRsnAsync(), Is.EqualTo("bottleo"), "first-run default");
        Assert.That(File.Exists(path), Is.True, "profile preference file exists");

        await store.SetSelectedRsnAsync("  Zezima  ");
        var reopened = new JsonProfilePreferenceStore(new ProfilePreferenceOptions
        {
            FilePath = path,
            DefaultRsn = "bottleo"
        });
        Assert.That(await reopened.GetSelectedRsnAsync(), Is.EqualTo("Zezima"), "persisted selected RSN");
        Assert.That(!File.Exists(path + ".tmp"), Is.True, "atomic profile temporary file replaced");
    }
}
