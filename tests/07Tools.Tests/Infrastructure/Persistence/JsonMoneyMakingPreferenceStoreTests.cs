namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
[Category("Persistence")]
public sealed class JsonMoneyMakingPreferenceStoreTests
{
    [Test]
    [Property("LegacyScenario", "MoneyMakingPreferencePersistence")]
    [Description("money-maker action-rate overrides persist atomically")]
    public async Task MoneyMakingPreferencePersistence()
    {
        using var temporary = new TemporaryDirectory();
        var directory = temporary.Path;
        var path = Path.Combine(directory, "money-making-preferences.json");
        var store = new JsonMoneyMakingPreferenceStore(
            new MoneyMakingPreferenceOptions { FilePath = path });

        await store.SetActionsPerHourOverrideAsync("Vyrewatch-Sentinels", 95m);
        await store.SetActionsPerHourOverrideAsync("zulrah", 27.5m);
        await store.SetBooleanOptionAsync(
            FrostDragonMethod.Slug,
            FrostDragonMethod.PickUpBonesOptionKey,
            false);

        var saved = await store.GetActionsPerHourOverridesAsync();
        Assert.That(saved["vyrewatch-sentinels"], Is.EqualTo(95m).Within(0m), "persisted Vyrewatch override");
        Assert.That(saved["ZULRAH"], Is.EqualTo(27.5m).Within(0m), "case-insensitive persisted override");
        var frostOptions = await store.GetBooleanOptionsAsync("FROST-DRAGONS-AFK-MELEE");
        Assert.That(!frostOptions[FrostDragonMethod.PickUpBonesOptionKey], Is.True, "case-insensitive Frost Dragon option persistence");

        await store.SetActionsPerHourOverrideAsync("zulrah", null);
        var afterReset = await store.GetActionsPerHourOverridesAsync();
        Assert.That(!afterReset.ContainsKey("zulrah"), Is.True, "reset removes persisted override");
        await store.SetBooleanOptionAsync(
            FrostDragonMethod.Slug,
            FrostDragonMethod.PickUpBonesOptionKey,
            null);
        Assert.That((await store.GetBooleanOptionsAsync(FrostDragonMethod.Slug)).Count == 0, Is.True, "default Frost Dragon option removes the persisted override");
        Assert.That(!File.Exists(path + ".tmp"), Is.True, "atomic preference replacement leaves no temporary file");
        await Assert.ThatAsync((Func<Task>)(() => store.SetActionsPerHourOverrideAsync("vyrewatch-sentinels", 0m)), Throws.InstanceOf<ArgumentOutOfRangeException>(), "non-positive action rate");
    }
}
