namespace RunescapeTools.Infrastructure.Market;

public static class WikiItemIconUriBuilder
{
    public static Uri Build(string wikiFileName)
    {
        if (string.IsNullOrWhiteSpace(wikiFileName))
            throw new ArgumentException("A Wiki icon filename is required.", nameof(wikiFileName));

        return new Uri(
            $"w/Special:Redirect/file/{Uri.EscapeDataString(wikiFileName.Trim())}",
            UriKind.Relative);
    }
}
