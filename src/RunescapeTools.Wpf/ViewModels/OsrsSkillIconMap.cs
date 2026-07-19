namespace RunescapeTools.Wpf.ViewModels;

public static class OsrsSkillIconMap
{
    private const string WikiImageBaseUrl = "https://oldschool.runescape.wiki/images/";

    private static readonly IReadOnlyDictionary<string, string> IconUrls =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Attack"] = WikiImageBaseUrl + "Attack_icon.png",
            ["Strength"] = WikiImageBaseUrl + "Strength_icon.png",
            ["Defence"] = WikiImageBaseUrl + "Defence_icon.png",
            ["Ranged"] = WikiImageBaseUrl + "Ranged_icon.png",
            ["Prayer"] = WikiImageBaseUrl + "Prayer_icon.png",
            ["Magic"] = WikiImageBaseUrl + "Magic_icon.png",
            ["Runecraft"] = WikiImageBaseUrl + "Runecraft_icon.png",
            ["Hitpoints"] = WikiImageBaseUrl + "Hitpoints_icon.png",
            ["Agility"] = WikiImageBaseUrl + "Agility_icon.png",
            ["Herblore"] = WikiImageBaseUrl + "Herblore_icon.png",
            ["Thieving"] = WikiImageBaseUrl + "Thieving_icon.png",
            ["Crafting"] = WikiImageBaseUrl + "Crafting_icon.png",
            ["Fletching"] = WikiImageBaseUrl + "Fletching_icon.png",
            ["Slayer"] = WikiImageBaseUrl + "Slayer_icon.png",
            ["Hunter"] = WikiImageBaseUrl + "Hunter_icon.png",
            ["Mining"] = WikiImageBaseUrl + "Mining_icon.png",
            ["Smithing"] = WikiImageBaseUrl + "Smithing_icon.png",
            ["Fishing"] = WikiImageBaseUrl + "Fishing_icon.png",
            ["Cooking"] = WikiImageBaseUrl + "Cooking_icon.png",
            ["Firemaking"] = WikiImageBaseUrl + "Firemaking_icon.png",
            ["Woodcutting"] = WikiImageBaseUrl + "Woodcutting_icon.png",
            ["Farming"] = WikiImageBaseUrl + "Farming_icon.png",
            ["Construction"] = WikiImageBaseUrl + "Construction_icon.png",
            ["Sailing"] = WikiImageBaseUrl + "Sailing_icon.png"
        };

    public static string? GetIconUrl(string? skillName)
    {
        if (string.IsNullOrWhiteSpace(skillName))
            return null;

        return IconUrls.TryGetValue(skillName.Trim(), out var iconUrl)
            ? iconUrl
            : null;
    }
}
