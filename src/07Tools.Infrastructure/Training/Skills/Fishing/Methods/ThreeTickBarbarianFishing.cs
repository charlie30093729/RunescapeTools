using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Fishing.Methods;

internal static class ThreeTickBarbarianFishing
{
    private const string DisplayName = "3-tick Barbarian Fishing";

    public static TrainingMethodDefinition Create() =>
        new(
            "three-tick-barbarian-fishing",
            DisplayName,
            FishingGlobal.WithMainRouteBeforeUnlock(
                FishingGlobal.CreateBarbarianBands(
                    DisplayName,
                    45_000m,
                    72_000m,
                    95_000m,
                    103_000m,
                    110_000m,
                    115_000m)),
            "Requires level 48 Fishing, level 15 Agility, level 15 Strength, and Barbarian " +
            "Training. Planner-calibrated rates assume standard herb-and-tar three-ticking " +
            "with fish dropped. Passive Agility XP is credited using the documented " +
            "level-banded Fishing-to-Agility ratios; Strength XP is informational only because " +
            "Strength is excluded from the planner. Bait and tick-manipulation supplies are not " +
            "priced, and the route does not assume cut-and-eat Cooking XP.",
            UseStableDisplayName: true);
}
