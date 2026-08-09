using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class ArceuusBloodRunes
{
    private const long UnlockExperience = 1_475_581;

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "arceuus-blood-runes",
            "Arceuus blood runes",
            [
                .. RunecraftGlobal.CreateBaseBands(),
                .. SoloMudRunes.CreateMethodBands(settings)
                    .Where(band => band.StartExperience < UnlockExperience),
                RunecraftGlobal.CreateArceuusRuneBand(
                    UnlockExperience,
                    36_000m,
                    23.8m,
                    "Arceuus blood runes",
                    Items.BloodRune,
                    settings)
            ],
            RunecraftGlobal.Note +
            " Requires level 77 Runecraft plus level 38 Mining and Crafting. Dark essence is gathered " +
            "at the dense essence mine and therefore has no tradeable input cost. The route excludes " +
            "blood essence, Kourend diary bonus runes, passive Mining and Crafting XP, ring charges, " +
            "and reusable equipment.");

    private static class Items
    {
        public static readonly CatalogueItem BloodRune = new(565, "Blood rune");
    }
}
