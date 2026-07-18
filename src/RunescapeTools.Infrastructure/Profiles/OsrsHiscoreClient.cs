using System.Net;
using RunescapeTools.Core.Profiles;

namespace RunescapeTools.Infrastructure.Profiles;

public sealed class OsrsHiscoreClient(HttpClient httpClient) : IHiscoreClient
{
    public async Task<string> GetRawHiscoresAsync(
        string rsn,
        CancellationToken cancellationToken = default)
    {
        var trimmedRsn = rsn?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedRsn))
            throw new ArgumentException("An RSN is required.", nameof(rsn));

        var requestUri = $"index_lite.ws?player={Uri.EscapeDataString(trimmedRsn)}";
        using var response = await httpClient.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new PlayerNotFoundException(trimmedRsn);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
