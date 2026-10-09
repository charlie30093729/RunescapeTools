namespace RunescapeTools.Infrastructure.Configuration;

public sealed class ProfilePreferenceOptions
{
    public required string FilePath { get; init; }

    public string DefaultRsn { get; init; } = "bottleo";
}
