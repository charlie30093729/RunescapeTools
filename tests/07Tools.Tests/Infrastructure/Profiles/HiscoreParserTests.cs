namespace RunescapeTools.Tests.Infrastructure.Profiles;

[TestFixture]
[Category("Unit")]
public sealed class HiscoreParserTests
{
    [Test]
    [Property("LegacyScenario", "HiscoreParserMapsSkills")]
    [Description("hiscore parser maps every current OSRS skill in API order")]
    public Task HiscoreParserMapsSkills()
    {
        var now = new DateTimeOffset(2026, 7, 18, 10, 30, 0, TimeSpan.Zero);
        var parser = new HiscoreParser(new TestTimeProvider(now));

        var profile = parser.Parse("  bottleo  ", HiscoreResponse());

        Assert.That(profile.Rsn, Is.EqualTo("bottleo"), "trimmed RSN");
        Assert.That(profile.OverallRank, Is.EqualTo(123), "overall rank");
        Assert.That(profile.TotalLevel, Is.EqualTo(2_376), "total level");
        Assert.That(profile.TotalExperience, Is.EqualTo(4_567_890_123L), "long total experience");
        Assert.That(profile.Skills.Count, Is.EqualTo(24), "current skill count");
        Assert.That(profile.Skills[0].Name, Is.EqualTo("Attack"), "first skill");
        Assert.That(profile.Skills[3].Name, Is.EqualTo("Hitpoints"), "API constitution alias");
        Assert.That(profile.Skills[20].Name, Is.EqualTo("Runecraft"), "API runecrafting alias");
        Assert.That(profile.Skills[^1].Name, Is.EqualTo("Sailing"), "latest skill");
        Assert.That(profile.RetrievedAtUtc, Is.EqualTo(now), "retrieval time");
        return Task.CompletedTask;
    }

    [Test]
    [Property("LegacyScenario", "HiscoreParserRejectsInvalidResponses")]
    [Description("hiscore parser rejects incomplete and malformed skill rows")]
    public async Task HiscoreParserRejectsInvalidResponses()
    {
        var parser = new HiscoreParser(TimeProvider.System);
        await Assert.ThatAsync((Func<Task>)(() => Task.FromResult(parser.Parse("bottleo", string.Join('\n', HiscoreResponse().Split('\n').Take(24))))), Throws.InstanceOf<HiscoreParseException>(), "incomplete response");

        var rows = HiscoreResponse().Split('\n');
        rows[5] = "not-a-rank,99,13034431";
        await Assert.ThatAsync((Func<Task>)(() => Task.FromResult(parser.Parse("bottleo", string.Join('\n', rows)))), Throws.InstanceOf<HiscoreParseException>(), "malformed response");
    }
}
