namespace RunescapeTools.Infrastructure.Configuration;

public sealed class OsrsWikiOptions
{
    public Uri BaseAddress { get; init; } = new("https://prices.runescape.wiki/api/v1/osrs/");

    public string UserAgent { get; init; } = "RunescapeTools/0.1 (contact: Discord bottleo)";

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);

    public int MaxRetryAttempts { get; init; } = 3;
}
