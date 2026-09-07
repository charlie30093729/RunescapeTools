namespace RunescapeTools.Tests.Infrastructure.Http;

[TestFixture]
[Category("Unit")]
public sealed class OsrsWikiPriceClientTests
{
    [Test]
    [Property("LegacyScenario", "WikiClientRetries")]
    [Description("Wiki client retries transient responses")]
    public async Task WikiClientRetries()
    {
        var handler = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"data\":{\"1\":{\"high\":120,\"low\":100,\"highTime\":1,\"lowTime\":1}}}", Encoding.UTF8, "application/json")
            });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var client = new OsrsWikiPriceClient(
            http,
            new OsrsWikiOptions { BaseAddress = http.BaseAddress, MaxRetryAttempts = 2 },
            NullLogger<OsrsWikiPriceClient>.Instance);

        var prices = await client.GetLatestAsync();

        Assert.That(handler.Calls, Is.EqualTo(2), "HTTP attempts");
        Assert.That(prices[1].MidPrice ?? 0, Is.EqualTo(110m).Within(0m), "retried quote midpoint");
    }
}
