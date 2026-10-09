namespace RunescapeTools.Core.Profiles;

public sealed record PlayerProfile(
    string Rsn,
    DateTimeOffset RetrievedAtUtc,
    int OverallRank,
    int TotalLevel,
    long TotalExperience,
    IReadOnlyList<PlayerSkill> Skills);

public sealed record PlayerSkill(
    string Name,
    int Rank,
    int Level,
    long Experience,
    int ApiOrder);
