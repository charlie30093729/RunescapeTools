namespace RunescapeTools.Infrastructure.Persistence;

/// <summary>Imports previous desktop state once, without modifying either existing data set.</summary>
public sealed class DesktopDataMigrator
{
    public const string ProductDirectoryName = "07Tools";

    public async Task MigrateAsync(string localAppData, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var productDirectory = Path.Combine(Path.GetFullPath(localAppData), ProductDirectoryName);
        var destination = Path.Combine(productDirectory, "data");
        if (Directory.Exists(destination))
            return;

        // These historical names are intentionally retained for existing installations.
        var previousData = Path.Combine(localAppData, "RunescapeTools", "data");
        var legacyFavourites = Path.Combine(localAppData, "RuneScapePriceChecker", "data", "favourites.json");
        Directory.CreateDirectory(productDirectory);
        var stagingDirectory = Path.Combine(productDirectory, $".data-migration-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stagingDirectory);

        try
        {
            if (Directory.Exists(previousData))
            {
                var enumeration = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = false,
                    AttributesToSkip = FileAttributes.ReparsePoint
                };
                foreach (var source in Directory.EnumerateFiles(previousData, "*", enumeration))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (source.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var target = Path.Combine(stagingDirectory, Path.GetRelativePath(previousData, source));
                    await CopyAsync(source, target, cancellationToken);
                }
            }

            var stagedFavourites = Path.Combine(stagingDirectory, "favourites.json");
            if (!File.Exists(stagedFavourites) && File.Exists(legacyFavourites))
                await CopyAsync(legacyFavourites, stagedFavourites, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                Directory.Move(stagingDirectory, destination);
            }
            catch (IOException) when (Directory.Exists(destination))
            {
                // Another initializer won the race. Its complete data takes precedence.
            }
        }
        finally
        {
            // Only this operation's private staging directory can be removed.
            if (Directory.Exists(stagingDirectory))
                Directory.Delete(stagingDirectory, recursive: true);
        }
    }

    private static async Task CopyAsync(string source, string destination, CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);
        await input.CopyToAsync(output, token);
        await output.FlushAsync(token);
    }
}
