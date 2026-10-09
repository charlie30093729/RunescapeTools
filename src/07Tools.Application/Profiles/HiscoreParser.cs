using System.Globalization;
using RunescapeTools.Core.Profiles;

namespace RunescapeTools.Application.Profiles;

public sealed class HiscoreParser(TimeProvider timeProvider)
{
    private const int OverallRowCount = 1;

    public PlayerProfile Parse(string rsn, string rawResponse)
    {
        var trimmedRsn = rsn?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRsn))
            throw new ArgumentException("An RSN is required.", nameof(rsn));
        if (string.IsNullOrWhiteSpace(rawResponse))
            throw new HiscoreParseException("The Old School Hiscores response was empty.");

        var rows = rawResponse
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var requiredRowCount = OverallRowCount + OsrsHiscoreSkillOrder.Skills.Count;
        if (rows.Length < requiredRowCount)
        {
            throw new HiscoreParseException(
                $"The Old School Hiscores response was incomplete: expected at least {requiredRowCount} rows but received {rows.Length}.");
        }

        var overall = ParseSkillRow(rows[0], 0, "Overall", allowZeroLevel: true);
        var skills = new PlayerSkill[OsrsHiscoreSkillOrder.Skills.Count];
        for (var index = 0; index < skills.Length; index++)
        {
            var name = OsrsHiscoreSkillOrder.Skills[index];
            var parsed = ParseSkillRow(rows[index + OverallRowCount], index + 1, name, allowZeroLevel: false);
            skills[index] = new PlayerSkill(name, parsed.Rank, parsed.Level, parsed.Experience, index);
        }

        return new PlayerProfile(
            trimmedRsn,
            timeProvider.GetUtcNow(),
            overall.Rank,
            overall.Level,
            overall.Experience,
            skills);
    }

    private static ParsedSkillRow ParseSkillRow(
        string row,
        int rowIndex,
        string label,
        bool allowZeroLevel)
    {
        var columns = row.Split(',', StringSplitOptions.TrimEntries);
        if (columns.Length != 3)
        {
            throw new HiscoreParseException(
                $"Hiscore row {rowIndex} ({label}) must contain rank, level, and experience.");
        }

        if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var rank)
            || !int.TryParse(columns[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var level)
            || !long.TryParse(columns[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var experience))
        {
            throw new HiscoreParseException(
                $"Hiscore row {rowIndex} ({label}) contains a malformed numeric value.");
        }

        if (rank < -1 || level < (allowZeroLevel ? 0 : 1) || experience < -1)
        {
            throw new HiscoreParseException(
                $"Hiscore row {rowIndex} ({label}) contains values outside the documented range.");
        }

        return new ParsedSkillRow(rank, level, experience);
    }

    private readonly record struct ParsedSkillRow(int Rank, int Level, long Experience);
}
