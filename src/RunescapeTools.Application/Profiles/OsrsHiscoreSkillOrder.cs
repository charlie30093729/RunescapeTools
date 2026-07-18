namespace RunescapeTools.Application.Profiles;

/// <summary>
/// Maps the skill rows returned by the standard Old School Hiscores Lite API.
/// Row zero is Overall; these skill names map rows one through twenty-four.
/// </summary>
public static class OsrsHiscoreSkillOrder
{
    public static IReadOnlyList<string> Skills { get; } =
    [
        "Attack",
        "Defence",
        "Strength",
        "Hitpoints",
        "Ranged",
        "Prayer",
        "Magic",
        "Cooking",
        "Woodcutting",
        "Fletching",
        "Fishing",
        "Firemaking",
        "Crafting",
        "Smithing",
        "Mining",
        "Herblore",
        "Agility",
        "Thieving",
        "Slayer",
        "Farming",
        "Runecraft",
        "Hunter",
        "Construction",
        "Sailing"
    ];
}
