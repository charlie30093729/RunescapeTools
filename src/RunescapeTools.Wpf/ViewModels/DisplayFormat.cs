namespace RunescapeTools.Wpf.ViewModels;

internal static class DisplayFormat
{
    public static string Gp(decimal? value) => value.HasValue ? $"{value.Value:N0} gp" : "Unavailable";

    public static string Gp(long? value) => value.HasValue ? $"{value.Value:N0} gp" : "Unavailable";

    public static string Quantity(decimal value) => value switch
    {
        < 1m => value.ToString("0.###"),
        < 100m => value.ToString("0.##"),
        _ => value.ToString("N0")
    };

    public static string Compact(long value) => value switch
    {
        >= 1_000_000_000 => $"{value / 1_000_000_000d:0.#}b",
        >= 1_000_000 => $"{value / 1_000_000d:0.#}m",
        >= 1_000 => $"{value / 1_000d:0.#}k",
        _ => value.ToString("N0")
    };

    public static string Monogram(string name) => string.Concat(
            name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => part[0]))
        .ToUpperInvariant();
}
