using System.Text.Json;
using RunescapePriceChecker.Core.Favourites;
using RunescapePriceChecker.Infrastructure.Configuration;

namespace RunescapePriceChecker.Infrastructure.Persistence;

public sealed class JsonFavouriteStore(FavouriteStoreOptions options) : IFavouriteStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath = Path.GetFullPath(options.FilePath);

    public async Task<IReadOnlyList<FavouriteItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            return (await LoadUnsafeAsync(cancellationToken))
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task AddAsync(FavouriteItem favourite, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(favourite);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var favourites = await LoadUnsafeAsync(cancellationToken);
            if (favourites.All(item => item.ItemId != favourite.ItemId))
            {
                favourites.Add(favourite);
                await SaveUnsafeAsync(favourites, cancellationToken);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task RemoveAsync(int itemId, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            var favourites = await LoadUnsafeAsync(cancellationToken);
            if (favourites.RemoveAll(item => item.ItemId == itemId) > 0)
                await SaveUnsafeAsync(favourites, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<List<FavouriteItem>> LoadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            var seed = DeserializeSeed();
            if (seed.Count > 0)
                await SaveUnsafeAsync(seed, cancellationToken);
            return seed;
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<FavouriteItem>>(
                   stream,
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    private List<FavouriteItem> DeserializeSeed()
    {
        if (string.IsNullOrWhiteSpace(options.SeedJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<FavouriteItem>>(options.SeedJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task SaveUnsafeAsync(List<FavouriteItem> favourites, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var sorted = favourites.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        var temporaryPath = filePath + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, sorted, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, filePath, true);
    }
}
