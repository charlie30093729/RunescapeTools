namespace RunescapeTools.Core.Profiles;

public interface IHiscoreClient
{
    Task<string> GetRawHiscoresAsync(
        string rsn,
        CancellationToken cancellationToken = default);
}

public sealed class PlayerNotFoundException(string rsn)
    : Exception($"No Old School RuneScape hiscores were found for '{rsn}'.")
{
    public string Rsn { get; } = rsn;
}

public sealed class HiscoreParseException(string message, Exception? innerException = null)
    : Exception(message, innerException);
