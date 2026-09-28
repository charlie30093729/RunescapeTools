using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter.Methods;

internal static class BlackChinchompas
{
    private const decimal BlackChinchompaExperience = 315m;

    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "Black chinchompas",
            [
                Band(0, 30_000m, "Varrock museum and birdhouses"),
                Band(2_107, 83_000m, "Oak birdhouses"),
                Band(7_028, 110_000m, "Willow birdhouses"),
                Band(20_224, 138_000m, "Teak birdhouses"),
                Band(55_649, 215_112m, "Drift net fishing"),
                Band(91_721, 268_770m, "Drift net fishing"),
                Band(184_040, 293_310m, "Drift net fishing"),
                Band(343_551, 322_424m, "Drift net fishing"),
                Band(737_627, 350_697m, "Drift net fishing"),
                Band(933_979, 275_000m, "Drift net fishing"),
                Band(
                    992_895,
                    265_000m,
                    "Black chinchompas",
                    new TrainingEconomics(
                        [Output(Items.BlackChinchompa, 1m / BlackChinchompaExperience)]))
            ],
            "Requires 73 Hunter and partial Eagles' Peak. Default: 3-tick hunting with a shooting alt, " +
            "265k XP/hour. Without the alt, 3-tick hunting uses 225k; without 3-ticking, the alt is " +
            "disabled and the user-approved planning preset is 200,000 XP/hour (about 635 catches/hour), " +
            "not a verified Wiki benchmark. These high-level planning " +
            "rates retain the legacy constant band from level 73, not measured lower-level rates. " +
            "Personal rates scale relative to these defaults when changing configuration. Catch XP " +
            "and chins per goal stay unchanged. Black chinchompas sell low after GE tax. " +
            "PK/death losses, tick-manipulation consumables and shooting-alt ammunition are excluded.",
            UseStableDisplayName: true);

    private static class Items
    {
        public static readonly CatalogueItem BlackChinchompa = new(11959, "Black chinchompa");
    }
}
