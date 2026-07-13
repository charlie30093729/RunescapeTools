using System.Net;
using System.Text.Json;
using RunescapePriceChecker.Core.Market;

namespace RunescapePriceChecker.Web.Infrastructure;

public sealed class OsrsWikiPriceClient(HttpClient httpClient, ILogger<OsrsWikiPriceClient> logger)
    : IOsrsPriceClient
{
    public async Task<IReadOnlyList<ItemMapping>> GetMappingAsync(CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync("mapping", cancellationToken);
        var items = new List<ItemMapping>(document.RootElement.GetArrayLength());

        foreach (var element in document.RootElement.EnumerateArray())
        {
            items.Add(new ItemMapping(
                element.GetProperty("id").GetInt32(),
                GetString(element, "name"),
                GetString(element, "examine"),
                element.TryGetProperty("members", out var members) && members.GetBoolean(),
                GetNullableInt(element, "limit"),
                GetString(element, "icon")));
        }

        return items;
    }

    public async Task<IReadOnlyDictionary<int, ItemPrice>> GetLatestAsync(
        CancellationToken cancellationToken = default)
    {
        using var document = await GetJsonAsync("latest", cancellationToken);
        var data = document.RootElement.GetProperty("data");
        var result = new Dictionary<int, ItemPrice>(4096);

        foreach (var property in data.EnumerateObject())
        {
            var itemId = int.Parse(property.Name);
            var value = property.Value;
            result[itemId] = new ItemPrice(
                itemId,
                GetNullableLong(value, "high"),
                GetNullableLong(value, "low"),
                GetUnixTime(value, "highTime"),
                GetUnixTime(value, "lowTime"));
        }

        return result;
    }

    public async Task<IReadOnlyList<PricePoint>> GetTimeSeriesAsync(
        int itemId,
        PriceTimeStep timeStep,
        CancellationToken cancellationToken = default)
    {
        if (itemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemId));

        var step = timeStep switch
        {
            PriceTimeStep.FiveMinutes => "5m",
            PriceTimeStep.OneHour => "1h",
            PriceTimeStep.SixHours => "6h",
            PriceTimeStep.TwentyFourHours => "24h",
            _ => throw new ArgumentOutOfRangeException(nameof(timeStep))
        };

        using var document = await GetJsonAsync(
            $"timeseries?id={itemId}&timestep={step}",
            cancellationToken);
        var result = new List<PricePoint>();

        foreach (var element in document.RootElement.GetProperty("data").EnumerateArray())
        {
            result.Add(new PricePoint(
                DateTimeOffset.FromUnixTimeSeconds(element.GetProperty("timestamp").GetInt64()),
                GetNullableLong(element, "avgHighPrice"),
                GetNullableLong(element, "avgLowPrice"),
                GetNullableLong(element, "highPriceVolume") ?? 0,
                GetNullableLong(element, "lowPriceVolume") ?? 0));
        }

        return result.OrderBy(point => point.Timestamp).ToArray();
    }

    private async Task<JsonDocument> GetJsonAsync(string path, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var response = await httpClient.GetAsync(path, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            }

            if (attempt < 3 && (response.StatusCode == HttpStatusCode.TooManyRequests
                                || (int)response.StatusCode >= 500))
            {
                var delay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(500 * attempt);
                logger.LogWarning(
                    "OSRS Wiki price request {Path} returned {StatusCode}; retrying in {Delay}.",
                    path,
                    (int)response.StatusCode,
                    delay);
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"OSRS Wiki price request failed with {(int)response.StatusCode}: {body}",
                null,
                response.StatusCode);
        }

        throw new HttpRequestException("OSRS Wiki price request failed after three attempts.");
    }

    private static string GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int? GetNullableInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;

    private static long? GetNullableLong(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt64()
            : null;

    private static DateTimeOffset? GetUnixTime(JsonElement element, string name)
    {
        var value = GetNullableLong(element, name);
        return value.HasValue ? DateTimeOffset.FromUnixTimeSeconds(value.Value) : null;
    }
}
