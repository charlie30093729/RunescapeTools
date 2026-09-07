namespace RunescapeTools.Tests.TestSupport.Builders;
internal static class ProfileTestData
{
    public static string HiscoreResponse(int skillLevel = 99)
    {
        var rows = new List<string> { "123,2376,4567890123" };
        rows.AddRange(Enumerable.Range(0, OsrsHiscoreSkillOrder.Skills.Count)
            .Select(index => $"{1_000 + index},{skillLevel},{13_034_431L + index}"));
        rows.Add("-1,-1"); // Activity rows may use rank,score and are intentionally ignored.
        return string.Join('\n', rows);
    }
}
