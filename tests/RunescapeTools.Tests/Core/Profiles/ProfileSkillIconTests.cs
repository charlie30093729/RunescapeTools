namespace RunescapeTools.Tests.Core.Profiles;

[TestFixture]
[Category("Unit")]
public sealed class ProfileSkillIconTests
{
    [Test]
    [Property("LegacyScenario", "ProfileSkillIconMapping")]
    [Description("profile skill icons map to official Wiki assets")]
    public Task ProfileSkillIconMapping()
    {
        foreach (var skill in OsrsHiscoreSkillOrder.Skills)
        {
            Assert.That(OsrsSkillIconMap.GetIconUrl(skill) ?? string.Empty, Is.EqualTo($"https://oldschool.runescape.wiki/images/{skill}_icon.png"), $"{skill} icon URL");
        }

        Assert.That(OsrsSkillIconMap.GetIconUrl("Runecraft") ?? string.Empty, Is.EqualTo("https://oldschool.runescape.wiki/images/Runecraft_icon.png"), "Runecraft uses the documented asset name");
        Assert.That(OsrsSkillIconMap.GetIconUrl("Runecrafting") is null, Is.True, "Runecrafting is not a valid display mapping");
        Assert.That(OsrsSkillIconMap.GetIconUrl("Unexpected skill") is null, Is.True, "unknown skills use the UI fallback");
        return Task.CompletedTask;
    }
}
