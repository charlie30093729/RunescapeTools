namespace RunescapeTools.Tests.Infrastructure.Persistence;

[TestFixture]
[Category("Persistence")]
public sealed class DesktopDataMigratorTests
{
    [Test]
    public async Task ImportsAllStateAndCachesWithoutChangingTheOriginalFiles()
    {
        using var temporary = new TemporaryDirectory();
        var files = new Dictionary<string, byte[]>
        {
            ["favourites.json"] = Encoding.UTF8.GetBytes("[{\"itemId\":24777,\"name\":\"Blood shard\"}]"),
            ["profile.json"] = Encoding.UTF8.GetBytes("{\"rsn\":\"Example player\"}"),
            ["training-plans.json"] = Encoding.UTF8.GetBytes("{\"goals\":[200000000]}"),
            ["money-making-preferences.json"] = Encoding.UTF8.GetBytes("{\"actionsPerHour\":88}"),
            [Path.Combine("price-history", "settings.json")] = Encoding.UTF8.GetBytes("{\"mode\":1}"),
            [Path.Combine("price-history", "24777-6h-v1.json")] = Encoding.UTF8.GetBytes("{\"points\":[]}"),
            [Path.Combine("item-icons", "24777.png")] = [137, 80, 78, 71, 0, 255]
        };
        foreach (var (relative, bytes) in files)
            await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", relative), bytes);
        await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "unfinished.tmp"), [1]);

        await new DesktopDataMigrator().MigrateAsync(temporary.Path);

        foreach (var (relative, bytes) in files)
        {
            Assert.That(await File.ReadAllBytesAsync(Path.Combine(temporary.Path, "07Tools", "data", relative)),
                Is.EqualTo(bytes), $"imported {relative}");
            Assert.That(await File.ReadAllBytesAsync(Path.Combine(temporary.Path, "RunescapeTools", "data", relative)),
                Is.EqualTo(bytes), $"preserved original {relative}");
        }
        Assert.That(File.Exists(Path.Combine(temporary.Path, "07Tools", "data", "unfinished.tmp")), Is.False);
        AssertNoStagingDirectory(temporary.Path);
    }

    [Test]
    public async Task ExistingDestinationIsNeverOverwrittenOrBackfilled()
    {
        using var temporary = new TemporaryDirectory();
        await WriteAsync(temporary.Path, Path.Combine("07Tools", "data", "favourites.json"), [1, 2]);
        await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "favourites.json"), [9]);
        await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "profile.json"), [8]);

        await new DesktopDataMigrator().MigrateAsync(temporary.Path);

        Assert.That(await File.ReadAllBytesAsync(Path.Combine(temporary.Path, "07Tools", "data", "favourites.json")),
            Is.EqualTo(new byte[] { 1, 2 }));
        Assert.That(File.Exists(Path.Combine(temporary.Path, "07Tools", "data", "profile.json")), Is.False,
            "A removed profile must not be resurrected from a previous installation.");
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task OldestFavouritesAreUsedOnlyWhenTheNewerFileIsMissing(bool newerFileExists)
    {
        using var temporary = new TemporaryDirectory();
        await WriteAsync(temporary.Path, Path.Combine("RuneScapePriceChecker", "data", "favourites.json"), [1]);
        await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "profile.json"), [3]);
        if (newerFileExists)
            await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "favourites.json"), [2]);

        await new DesktopDataMigrator().MigrateAsync(temporary.Path);

        Assert.That(await File.ReadAllBytesAsync(Path.Combine(temporary.Path, "07Tools", "data", "favourites.json")),
            Is.EqualTo(new byte[] { newerFileExists ? (byte)2 : (byte)1 }));
    }

    [Test]
    public async Task FreshInstallLeavesFavouritesForTheNormalSeedAndDoesNotImportLater()
    {
        using var temporary = new TemporaryDirectory();
        var migrator = new DesktopDataMigrator();
        await migrator.MigrateAsync(temporary.Path);
        var destination = Path.Combine(temporary.Path, "07Tools", "data");
        Assert.That(Directory.Exists(destination), Is.True);
        Assert.That(Directory.GetFiles(destination), Is.Empty);

        await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "profile.json"), [5]);
        await migrator.MigrateAsync(temporary.Path);
        Assert.That(Directory.GetFiles(destination), Is.Empty);
    }

    [Test]
    public async Task CancellationDoesNotCreateADestinationOrChangeTheSource()
    {
        using var temporary = new TemporaryDirectory();
        var source = await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "profile.json"), [7]);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThatAsync(
            (Func<Task>)(() => new DesktopDataMigrator().MigrateAsync(temporary.Path, cancellation.Token)),
            Throws.InstanceOf<OperationCanceledException>());

        Assert.That(Directory.Exists(Path.Combine(temporary.Path, "07Tools", "data")), Is.False);
        Assert.That(await File.ReadAllBytesAsync(source), Is.EqualTo(new byte[] { 7 }));
    }

    [Test]
    public async Task FailedCopyLeavesNoPartialDestinationAndCanBeRetried()
    {
        using var temporary = new TemporaryDirectory();
        await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "favourites.json"), [1]);
        var lockedPath = await WriteAsync(temporary.Path, Path.Combine("RunescapeTools", "data", "profile.json"), [2]);
        var migrator = new DesktopDataMigrator();
        using (var locked = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            await Assert.ThatAsync((Func<Task>)(() => migrator.MigrateAsync(temporary.Path)), Throws.InstanceOf<IOException>());
        }

        Assert.That(Directory.Exists(Path.Combine(temporary.Path, "07Tools", "data")), Is.False);
        AssertNoStagingDirectory(temporary.Path);
        await migrator.MigrateAsync(temporary.Path);
        Assert.That(await File.ReadAllBytesAsync(Path.Combine(temporary.Path, "07Tools", "data", "profile.json")),
            Is.EqualTo(new byte[] { 2 }));
    }

    private static async Task<string> WriteAsync(string root, string relative, byte[] contents)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, contents);
        return path;
    }

    private static void AssertNoStagingDirectory(string root) => Assert.That(
        Directory.GetDirectories(Path.Combine(root, "07Tools"), ".data-migration-*"), Is.Empty);
}
