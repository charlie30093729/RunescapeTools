namespace RunescapeTools.Infrastructure.Configuration;

public sealed class OsrsHiscoreOptions
{
    public Uri BaseAddress { get; init; } = new("https://secure.runescape.com/m=hiscore_oldschool/");

    public string UserAgent { get; init; } = "RunescapeTools/0.1 (contact: Discord bottleo)";

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);
}
