using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Fishing.Methods;

internal static class FiveTickBarbarianFishing
{
    private const string DisplayName = "5-tick Barbarian Fishing";

    public static TrainingMethodDefinition Create() =>
        new(
            "five-tick-barbarian-fishing",
            DisplayName,
            FishingGlobal.WithMainRouteBeforeUnlock(
                FishingGlobal.CreateBarbarianBands(
                    DisplayName,
                    25_000m,
                    40_000m,
                    52_000m,
                    56_000m,
                    58_000m,
                    60_000m)),
            "Requires level 48 Fishing, level 15 Agility, level 15 Strength, and Barbarian " +
            "Training. Planner-calibrated rates assume standard five-tick catches with fish " +
            "dropped. Passive Agility XP is credited using the documented level-banded " +
            "Fishing-to-Agility ratios; Strength XP is informational only because Strength is " +
            "excluded from the planner. Bait is not priced.",
            UseStableDisplayName: true);
}
