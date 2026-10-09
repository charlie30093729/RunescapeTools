namespace RunescapeTools.Tests.Infrastructure.Icons;

[TestFixture]
[Category("Persistence")]
public sealed class ItemIconServiceTests
{
    [Test]
    [Property("LegacyScenario", "ItemIconCachePersistence")]
    [Description("item icons resolve by ID and persist in the local cache")]
    public async Task ItemIconCachePersistence()
    {
        using var temporary = new TemporaryDirectory();
        var directory = temporary.Path;
        var mapping = new ItemMapping(
            3002,
            "Toadflax potion (unf)",
            string.Empty,
            true,
            10_000,
            "Toadflax potion (unf).png");
        var market = new FakeMarketDataService { Mappings = [mapping] };
        var imageBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10, 1, 2, 3, 4 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageBytes)
        };
        response.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        var handler = new SequenceHandler(response);
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://oldschool.runescape.wiki/")
        };
        var options = new ItemIconCacheOptions { DirectoryPath = directory };
        var service = new WikiItemIconService(
            http,
            market,
            options,
            NullLogger<WikiItemIconService>.Instance);

        var first = await service.GetAsync(mapping.Id);
        var second = await service.GetAsync(mapping.Id);

        Assert.That(first is not null, Is.True, "mapped item icon should resolve");
        Assert.That(second?.LocalFilePath ?? string.Empty, Is.EqualTo(first!.LocalFilePath), "same cache path is reused");
        Assert.That(handler.Calls, Is.EqualTo(1), "icon is downloaded only once");
        Assert.That(File.Exists(first.LocalFilePath), Is.True, "downloaded icon is persisted");
        Assert.That((await File.ReadAllBytesAsync(first.LocalFilePath)).SequenceEqual(imageBytes), Is.True, "persisted icon bytes");
        Assert.That(handler.LastRequestUri?.AbsoluteUri.Contains(
                "Special:Redirect/file/Toadflax%20potion%20%28unf%29.png",
                StringComparison.Ordinal) == true, Is.True, "Wiki filename is URL-encoded through the stable file redirect");

        var reopenedHandler = new SequenceHandler();
        var reopened = new WikiItemIconService(
            new HttpClient(reopenedHandler)
            {
                BaseAddress = new Uri("https://oldschool.runescape.wiki/")
            },
            market,
            options,
            NullLogger<WikiItemIconService>.Instance);
        var cached = await reopened.GetAsync(mapping.Id);

        Assert.That(cached?.LocalFilePath ?? string.Empty, Is.EqualTo(first.LocalFilePath), "cache survives service restart");
        Assert.That(reopenedHandler.Calls, Is.EqualTo(0), "persisted icon avoids a second HTTP request");
    }
}
