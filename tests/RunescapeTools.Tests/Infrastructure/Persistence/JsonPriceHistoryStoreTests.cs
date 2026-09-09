using static RunescapeTools.Tests.Core.Market.HistoricalPriceCalculatorTests;

namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
public sealed class JsonPriceHistoryStoreTests
{
    [Test]
    public async Task HistoryAndPreferenceRoundTripWithoutTemporaryFiles()
    {
        using var folder = new TemporaryDirectory();
        var store = new JsonPriceHistoryStore(new(folder.Path));
        Assert.That(await store.ReadModeAsync(default), Is.EqualTo(PricingMode.Live));
        Assert.That(await store.ReadAsync(1, default), Is.Null);
        await store.WriteAsync(1, new(End, History()), default);
        await store.WriteModeAsync(PricingMode.ThirtyDayAverage, default);
        var reopened = new JsonPriceHistoryStore(new(folder.Path));
        Assert.That((await reopened.ReadAsync(1, default))!.Points, Is.EqualTo(History()));
        Assert.That(await reopened.ReadModeAsync(default), Is.EqualTo(PricingMode.ThirtyDayAverage));
        Assert.That(Directory.GetFiles(folder.Path, "*.tmp"), Is.Empty);
    }

    [Test]
    public async Task CancelledWriteDoesNotReplaceExistingHistory()
    {
        using var folder = new TemporaryDirectory();
        var store = new JsonPriceHistoryStore(new(folder.Path));
        await store.WriteAsync(1, new(End, History()), default);
        using var token = new CancellationTokenSource(); token.Cancel();
        Assert.That((Func<Task>)(async () =>
            await store.WriteAsync(1, new(End.AddHours(1), []), token.Token)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That((await store.ReadAsync(1, default))!.FetchedAt, Is.EqualTo(End));
        Assert.That(Directory.GetFiles(folder.Path, "*.tmp"), Is.Empty);
    }

    [TestCase("invalid JSON")]
    [TestCase("{\"FetchedAt\":\"2026-09-09T12:00:00Z\",\"Points\":null}")]
    public async Task CorruptHistoryIsACacheMiss(string text)
    {
        using var folder = new TemporaryDirectory();
        await File.WriteAllTextAsync(Path.Combine(folder.Path, "1-6h-v1.json"), text);
        Assert.That(await new JsonPriceHistoryStore(new(folder.Path)).ReadAsync(1, default), Is.Null);
    }
}
