using System.Text.Json;
using RunescapePriceChecker.Core.Favourites;

namespace RunescapePriceChecker.Web.Infrastructure;

public sealed class JsonFavouriteStore : IFavouriteStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath;

    public JsonFavouriteStore(IHostEnvironment environment, IConfiguration configuration)
    {
        var dataDirectory = configuration["DataDirectory"] ?? "data";
        filePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, dataDirectory, "favourites.json"));
    }

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
            return [];

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<FavouriteItem>>(
                   stream,
                   JsonOptions,
                   cancellationToken)
               ?? [];
    }

    private async Task SaveUnsafeAsync(List<FavouriteItem> favourites, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var temporaryPath = filePath + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, favourites, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, filePath, true);
    }
}
