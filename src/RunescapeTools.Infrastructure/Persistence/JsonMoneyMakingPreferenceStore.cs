using System.Text.Json;
using RunescapeTools.Application.MoneyMaking;
using RunescapeTools.Infrastructure.Configuration;

namespace RunescapeTools.Infrastructure.Persistence;

public sealed class JsonMoneyMakingPreferenceStore(MoneyMakingPreferenceOptions options)
    : IMoneyMakingPreferenceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath = Path.GetFullPath(options.FilePath);

    public async Task<IReadOnlyDictionary<string, decimal>> GetActionsPerHourOverridesAsync(
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var state = await ReadUnsafeAsync(cancellationToken);
            return (state.ActionsPerHourOverrides ?? [])
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0m)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SetActionsPerHourOverrideAsync(
        string methodSlug,
        decimal? actionsPerHour,
        CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(methodSlug);
        if (actionsPerHour is <= 0m)
            throw new ArgumentOutOfRangeException(
                nameof(actionsPerHour),
                "Actions per hour must be greater than zero.");

        await gate.WaitAsync(cancellationToken);
        try
        {
            var state = await ReadUnsafeAsync(cancellationToken);
            state.ActionsPerHourOverrides ??=
                new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (actionsPerHour.HasValue)
                state.ActionsPerHourOverrides[slug] = actionsPerHour.Value;
            else
                state.ActionsPerHourOverrides.Remove(slug);
            state.ActionsPerHourOverrides = state.ActionsPerHourOverrides
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.OrdinalIgnoreCase);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            var temporaryPath = filePath + ".tmp";
            await using (var stream = File.Create(temporaryPath))
                await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
            File.Move(temporaryPath, filePath, true);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<MoneyMakingPreferenceState> ReadUnsafeAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return new MoneyMakingPreferenceState();

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<MoneyMakingPreferenceState>(
                   stream,
                   JsonOptions,
                   cancellationToken)
               ?? new MoneyMakingPreferenceState();
    }

    private static string NormalizeSlug(string methodSlug)
    {
        var value = methodSlug?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A money-making method slug is required.", nameof(methodSlug));
        return value.ToLowerInvariant();
    }

    private sealed class MoneyMakingPreferenceState
    {
        public Dictionary<string, decimal>? ActionsPerHourOverrides { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
