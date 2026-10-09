using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class ArceuusSoulRunes
{
    private const long UnlockExperience = 5_346_332;

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "arceuus-soul-runes",
            "Arceuus soul runes",
            [
                .. RunecraftGlobal.CreateBaseBands(),
                .. SoloMudRunes.CreateMethodBands(settings)
                    .Where(band => band.StartExperience < UnlockExperience),
                RunecraftGlobal.CreateArceuusRuneBand(
                    UnlockExperience,
                    44_000m,
                    29.7m,
                    "Arceuus soul runes",
                    Items.SoulRune,
                    settings)
            ],
            RunecraftGlobal.Note +
            " Requires level 90 Runecraft plus level 38 Mining and Crafting. Dark essence is gathered " +
            "at the dense essence mine and therefore has no tradeable input cost. The route excludes " +
            "Kourend diary bonus blocks, passive Mining and Crafting XP, ring charges, and reusable " +
            "equipment.");

    private static class Items
    {
        public static readonly CatalogueItem SoulRune = new(566, "Soul rune");
    }
}
