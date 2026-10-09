namespace RunescapeTools.Tests.Infrastructure.Http;

[TestFixture]
[Category("Unit")]
public sealed class HiscoreClientTests
{
    [Test]
    [Property("LegacyScenario", "HiscoreClientProtocol")]
    [Description("hiscore client URL-encodes RSNs and distinguishes missing accounts")]
    public async Task HiscoreClientProtocol()
    {
        var successHandler = new SequenceHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(HiscoreResponse(), Encoding.UTF8, "text/plain")
        });
        using var successHttp = new HttpClient(successHandler)
        {
            BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/")
        };
        var client = new OsrsHiscoreClient(successHttp);

        await client.GetRawHiscoresAsync("  Name With Space  ");

        Assert.That(successHandler.LastRequestUri?.AbsoluteUri.EndsWith("index_lite.ws?player=Name%20With%20Space", StringComparison.Ordinal) == true, Is.True, "URL-encoded standard endpoint");

        var missingHandler = new SequenceHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        using var missingHttp = new HttpClient(missingHandler) { BaseAddress = successHttp.BaseAddress };
        await Assert.ThatAsync((Func<Task>)(() => new OsrsHiscoreClient(missingHttp).GetRawHiscoresAsync("Missing Player")), Throws.InstanceOf<PlayerNotFoundException>(), "not-found response");
    }
}
