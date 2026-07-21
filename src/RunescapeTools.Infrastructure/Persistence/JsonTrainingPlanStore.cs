using System.Text.Json;
using RunescapeTools.Application.Training;
using RunescapeTools.Infrastructure.Configuration;

namespace RunescapeTools.Infrastructure.Persistence;

public sealed class JsonTrainingPlanStore(TrainingPlanOptions options) : ITrainingPlanStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string filePath = Path.GetFullPath(options.FilePath);

    public async Task<IReadOnlyDictionary<string, TrainingSkillPreference>> GetAsync(
        string rsn,
        CancellationToken cancellationToken = default)
    {
        var key = NormalizeRsn(rsn);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var state = await ReadUnsafeAsync(cancellationToken);
            return state.Profiles.TryGetValue(key, out var values)
                ? values.ToDictionary(value => value.Skill, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, TrainingSkillPreference>(StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(
        string rsn,
        IReadOnlyCollection<TrainingSkillPreference> preferences,
        CancellationToken cancellationToken = default)
    {
        var key = NormalizeRsn(rsn);
        await gate.WaitAsync(cancellationToken);
        try
        {
            var state = await ReadUnsafeAsync(cancellationToken);
            state.Profiles[key] = preferences.OrderBy(value => value.Skill).ToList();

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

    private async Task<TrainingPlanState> ReadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
            return new TrainingPlanState();

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<TrainingPlanState>(stream, JsonOptions, cancellationToken)
               ?? new TrainingPlanState();
    }

    private static string NormalizeRsn(string rsn)
    {
        var value = rsn?.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("An RSN is required.", nameof(rsn));
        return value.ToLowerInvariant();
    }

    private sealed class TrainingPlanState
    {
        public Dictionary<string, List<TrainingSkillPreference>> Profiles { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
