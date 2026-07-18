namespace RunescapeTools.Infrastructure.Configuration;

public sealed class FavouriteStoreOptions
{
    public required string FilePath { get; init; }

    public string? SeedJson { get; init; }
}
