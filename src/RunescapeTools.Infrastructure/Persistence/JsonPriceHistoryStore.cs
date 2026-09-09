using System.Text.Json;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Market;

namespace RunescapeTools.Infrastructure.Persistence;

public sealed record PriceHistoryStoreOptions(string DirectoryPath);

public sealed class JsonPriceHistoryStore(PriceHistoryStoreOptions options) : IPriceHistoryStore
{
    private readonly string directory = Path.GetFullPath(options.DirectoryPath);
    private readonly SemaphoreSlim settingsGate = new(1, 1);

    public async Task<CachedPriceHistory?> ReadAsync(int itemId, CancellationToken cancellationToken)
    {
        var history = await ReadJsonAsync<CachedPriceHistory>(ItemPath(itemId), cancellationToken);
        return history?.Points is null || history.Points.Any(p => p is null) ? null : history;
    }
    public Task WriteAsync(int itemId, CachedPriceHistory history, CancellationToken cancellationToken) =>
        WriteJsonAsync(ItemPath(itemId), history, cancellationToken);

    public async Task<PricingMode> ReadModeAsync(CancellationToken cancellationToken)
    {
        var settings = await ReadJsonAsync<Settings>(Path.Combine(directory, "settings.json"), cancellationToken);
        return settings is not null && Enum.IsDefined(settings.Mode) ? settings.Mode : PricingMode.Live;
    }

    public async Task WriteModeAsync(PricingMode mode, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        await settingsGate.WaitAsync(cancellationToken);
        try { await WriteJsonAsync(Path.Combine(directory, "settings.json"), new Settings(mode), cancellationToken); }
        finally { settingsGate.Release(); }
    }

    private string ItemPath(int id) => id > 0 ? Path.Combine(directory, $"{id}-6h-v1.json")
        : throw new ArgumentOutOfRangeException(nameof(id));

    private static async Task<T?> ReadJsonAsync<T>(string path, CancellationToken token)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
        }
        catch (FileNotFoundException) { return default; }
        catch (DirectoryNotFoundException) { return default; }
        catch (JsonException) { return default; } // Disposable cache; refetch corrupt entries.
    }

    private static async Task WriteJsonAsync<T>(string path, T value, CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = File.Create(temporary))
                await JsonSerializer.SerializeAsync(stream, value, cancellationToken: token);
            token.ThrowIfCancellationRequested();
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private sealed record Settings(PricingMode Mode);
}
