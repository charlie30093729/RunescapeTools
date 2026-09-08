namespace RunescapeTools.Tests.TestSupport.Builders;

internal static class PrayerOfferingTestData
{
    public static TrainingSkillDefinition CreatePrayerDefinition() =>
        new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Prayer");

    public static Dictionary<string, string> Location(string location) =>
        new() { ["offering-location"] = location };

    public static Dictionary<int, ItemPrice> Prices() => new()
    {
        [536] = new(536, 3_000, 2_000, null, null),
        [22124] = new(22124, 10_000, 9_000, null, null),
        [31729] = new(31729, 6_000, 5_000, null, null),
        [565] = new(565, 400, 300, null, null),
        [21880] = new(21880, 500, 400, null, null),
        [12695] = new(12695, 12_000, 11_000, null, null),
        [23685] = new(23685, 16_000, 15_000, null, null)
    };
}
