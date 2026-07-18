using System.Text.Json;
using RunescapeTools.Application.Profiles;
using RunescapeTools.Infrastructure.Configuration;

namespace RunescapeTools.Infrastructure.Persistence;

public sealed class JsonProfilePreferenceStore(ProfilePreferenceOptions options) : IProfilePreferenceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath = Path.GetFullPath(options.FilePath);
    private readonly string defaultRsn = ValidateRsn(options.DefaultRsn);

    public async Task<string> GetSelectedRsnAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(filePath))
            {
                await using var stream = File.OpenRead(filePath);
                var preference = await JsonSerializer.DeserializeAsync<ProfilePreference>(
                    stream,
                    JsonOptions,
                    cancellationToken);
                if (preference is not null && !string.IsNullOrWhiteSpace(preference.SelectedRsn))
                    return preference.SelectedRsn.Trim();
            }

            await SaveUnsafeAsync(defaultRsn, cancellationToken);
            return defaultRsn;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SetSelectedRsnAsync(string rsn, CancellationToken cancellationToken = default)
    {
        var validatedRsn = ValidateRsn(rsn);
        await gate.WaitAsync(cancellationToken);
        try
        {
            await SaveUnsafeAsync(validatedRsn, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task SaveUnsafeAsync(string rsn, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var temporaryPath = filePath + ".tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                new ProfilePreference(rsn),
                JsonOptions,
                cancellationToken);
        }

        File.Move(temporaryPath, filePath, true);
    }

    private static string ValidateRsn(string rsn)
    {
        var trimmed = rsn?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("A selected RSN is required.", nameof(rsn));
        return trimmed;
    }

    private sealed record ProfilePreference(string SelectedRsn);
}
