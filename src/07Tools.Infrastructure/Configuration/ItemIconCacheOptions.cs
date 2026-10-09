namespace RunescapeTools.Infrastructure.Configuration;

public sealed class ItemIconCacheOptions
{
    public required string DirectoryPath { get; init; }

    public Uri WikiBaseAddress { get; init; } = new("https://oldschool.runescape.wiki/");

    public int MaximumIconBytes { get; init; } = 2 * 1024 * 1024;

    public int MaximumConcurrentDownloads { get; init; } = 4;
}
