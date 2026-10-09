using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Agility.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Agility;

internal static class AgilityGlobal
{
    public const long Level75Experience = 1_210_421;
    public const long Level80Experience = 1_986_068;
    public const long Level85Experience = 3_258_594;
    public const long Level90Experience = 5_346_332;

    // Retain the existing default route before an alternative's real unlock.
    // This does not introduce new lower-level Sepulchre assumptions.
    public static IReadOnlyList<TrainingRateBand> WithMainRouteBeforeUnlock(
        params TrainingRateBand[] bands) =>
        HallowedSepulchre.Create().Bands
            .Where(band => band.StartExperience < bands[0].StartExperience)
            .Concat(bands)
            .ToArray();
}
