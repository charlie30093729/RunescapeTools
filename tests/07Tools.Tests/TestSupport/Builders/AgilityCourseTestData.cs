namespace RunescapeTools.Tests.TestSupport.Builders;

internal static class AgilityCourseTestData
{
    public static TrainingSkillDefinition Definition() =>
        new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Agility");

    public static Dictionary<int, ItemPrice> Prices()
    {
        var prices = PrayerOfferingTestData.Prices();
        prices[12640] = new(12640, 1200, 1000, null, null);
        return prices;
    }
}
