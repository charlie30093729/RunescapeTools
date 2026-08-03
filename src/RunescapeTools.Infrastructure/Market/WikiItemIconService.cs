using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using RunescapeTools.Application.Market;
using RunescapeTools.Core.Market;
using RunescapeTools.Infrastructure.Configuration;

namespace RunescapeTools.Infrastructure.Market;

public sealed class WikiItemIconService : IItemIconService
{
    private readonly HttpClient httpClient;
    private readonly IMarketDataService marketData;
    private readonly ItemIconCacheOptions options;
    private readonly ILogger<WikiItemIconService> logger;
    private readonly SemaphoreSlim downloadGate;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> fileGates = new(StringComparer.OrdinalIgnoreCase);

    public WikiItemIconService(
        HttpClient httpClient,
        IMarketDataService marketData,
        ItemIconCacheOptions options,
        ILogger<WikiItemIconService> logger)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaximumIconBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.MaximumConcurrentDownloads);

        this.httpClient = httpClient;
        this.marketData = marketData;
        this.options = options;
        this.logger = logger;
        downloadGate = new SemaphoreSlim(options.MaximumConcurrentDownloads);
    }

    public async Task<ItemIcon?> GetAsync(
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var icons = await GetManyAsync([itemId], cancellationToken);
        return icons.GetValueOrDefault(itemId);
    }

    public async Task<IReadOnlyDictionary<int, ItemIcon>> GetManyAsync(
        IEnumerable<int> itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var ids = itemIds.Where(id => id > 0).Distinct().ToArray();
        if (ids.Length == 0)
            return new Dictionary<int, ItemIcon>();

        try
        {
            var mappings = await marketData.GetItemMappingsAsync(ids, cancellationToken);
            var tasks = mappings.Values
                .Where(mapping => !string.IsNullOrWhiteSpace(mapping.Icon))
                .Select(mapping => GetOrDownloadAsync(mapping, cancellationToken));
            var icons = await Task.WhenAll(tasks);
            return icons
                .Where(icon => icon is not null)
                .ToDictionary(icon => icon!.ItemId, icon => icon!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "OSRS item icons could not be resolved.");
            return new Dictionary<int, ItemIcon>();
        }
    }

    private async Task<ItemIcon?> GetOrDownloadAsync(
        ItemMapping mapping,
        CancellationToken cancellationToken)
    {
        var path = GetCachePath(mapping);
        if (IsUsableCacheFile(path))
            return new ItemIcon(mapping.Id, mapping.Icon, path);

        var fileGate = fileGates.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
        await fileGate.WaitAsync(cancellationToken);
        try
        {
            if (IsUsableCacheFile(path))
                return new ItemIcon(mapping.Id, mapping.Icon, path);

            await downloadGate.WaitAsync(cancellationToken);
            try
            {
                return await DownloadAsync(mapping, path, cancellationToken)
                    ? new ItemIcon(mapping.Id, mapping.Icon, path)
                    : null;
            }
            finally
            {
                downloadGate.Release();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "The Wiki icon for item {ItemId} ({ItemName}) could not be cached.",
                mapping.Id,
                mapping.Name);
            return null;
        }
        finally
        {
            fileGate.Release();
        }
    }

    private async Task<bool> DownloadAsync(
        ItemMapping mapping,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            WikiItemIconUriBuilder.Build(mapping.Icon),
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogDebug(
                "Wiki item icon request for {ItemId} returned {StatusCode}.",
                mapping.Id,
                response.StatusCode);
            return false;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is not null
            && !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Wiki item icon request for {ItemId} returned unexpected content type {ContentType}.",
                mapping.Id,
                mediaType);
            return false;
        }

        if (response.Content.Headers.ContentLength > options.MaximumIconBytes)
            return false;

        Directory.CreateDirectory(options.DirectoryPath);
        var temporaryPath = destinationPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using (var destination = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             81920,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[81920];
                var total = 0;
                while (true)
                {
                    var read = await source.ReadAsync(buffer, cancellationToken);
                    if (read == 0)
                        break;

                    total += read;
                    if (total > options.MaximumIconBytes)
                        return false;

                    await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (total == 0)
                    return false;
            }

            try
            {
                File.Move(temporaryPath, destinationPath, overwrite: false);
            }
            catch (IOException) when (IsUsableCacheFile(destinationPath))
            {
                // Another request completed the same immutable cache entry first.
            }

            return IsUsableCacheFile(destinationPath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private string GetCachePath(ItemMapping mapping)
    {
        var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(mapping.Icon)))
            [..12]
            .ToLowerInvariant();
        var extension = Path.GetExtension(mapping.Icon);
        if (extension.Length is 0 or > 8
            || extension.Any(character => !char.IsLetterOrDigit(character) && character != '.'))
        {
            extension = ".img";
        }

        return Path.Combine(options.DirectoryPath, $"{mapping.Id}-{hash}{extension.ToLowerInvariant()}");
    }

    private bool IsUsableCacheFile(string path)
    {
        try
        {
            var file = new FileInfo(path);
            return file.Exists && file.Length is > 0 && file.Length <= options.MaximumIconBytes;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
