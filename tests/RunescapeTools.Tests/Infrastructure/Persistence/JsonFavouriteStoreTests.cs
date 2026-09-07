namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
[Category("Persistence")]
public sealed class JsonFavouriteStoreTests
{
    [Test]
    [Property("LegacyScenario", "JsonStoreSeedsSortsAndDeduplicates")]
    [Description("JSON store seeds, sorts, and prevents duplicates")]
    public async Task JsonStoreSeedsSortsAndDeduplicates()
    {
        using var temporary = new TemporaryDirectory();
        var directory = temporary.Path;
        var path = Path.Combine(directory, "favourites.json");
        var store = new JsonFavouriteStore(new FavouriteStoreOptions
        {
            FilePath = path,
            SeedJson = "[{\"itemId\":2,\"name\":\"Zulrah scale\",\"addedAt\":\"2026-01-01T00:00:00Z\"},{\"itemId\":1,\"name\":\"Blood shard\",\"addedAt\":\"2026-01-01T00:00:00Z\"}]"
        });

        var seeded = await store.GetAllAsync();
        await store.AddAsync(new FavouriteItem(1, "Duplicate", DateTimeOffset.UtcNow));
        await store.AddAsync(new FavouriteItem(3, "Adamant bar", DateTimeOffset.UtcNow));
        var saved = await store.GetAllAsync();

        Assert.That(seeded[0].Name, Is.EqualTo("Blood shard"), "seed sort order");
        Assert.That(saved.Count, Is.EqualTo(3), "duplicate prevention");
        Assert.That(saved[0].Name, Is.EqualTo("Adamant bar"), "persisted sort order");
        Assert.That(File.Exists(path), Is.True, "favourites file exists");
        Assert.That(!File.Exists(path + ".tmp"), Is.True, "atomic temporary file is replaced");
    }

    [Test]
    [Property("LegacyScenario", "JsonStoreDoesNotOverwrite")]
    [Description("JSON store never overwrites existing state")]
    public async Task JsonStoreDoesNotOverwrite()
    {
        using var temporary = new TemporaryDirectory();
        var directory = temporary.Path;
        var path = Path.Combine(directory, "favourites.json");
        await File.WriteAllTextAsync(path, "[{\"itemId\":9,\"name\":\"Existing\",\"addedAt\":\"2026-01-01T00:00:00Z\"}]");
        var store = new JsonFavouriteStore(new FavouriteStoreOptions
        {
            FilePath = path,
            SeedJson = "[{\"itemId\":1,\"name\":\"Seed\",\"addedAt\":\"2026-01-01T00:00:00Z\"}]"
        });

        var items = await store.GetAllAsync();

        Assert.That(items.Count, Is.EqualTo(1), "existing state count");
        Assert.That(items[0].ItemId, Is.EqualTo(9), "existing item retained");
    }
}
