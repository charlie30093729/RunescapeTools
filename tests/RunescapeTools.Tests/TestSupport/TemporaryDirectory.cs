namespace RunescapeTools.Tests.TestSupport;

/// <summary>Owns only its unique test directory, never the application's persisted user data.</summary>
internal sealed class TemporaryDirectory : IDisposable
{
    private readonly string parent = System.IO.Path.GetFullPath(
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RunescapeTools.Tests"));

    public TemporaryDirectory()
    {
        Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(parent, Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (!string.Equals(System.IO.Path.GetDirectoryName(Path), parent, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing to clean up outside the test directory.");

        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
