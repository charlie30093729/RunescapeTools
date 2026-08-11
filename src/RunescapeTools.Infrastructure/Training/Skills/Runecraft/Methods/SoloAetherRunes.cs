using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class SoloAetherRunes
{
    private const decimal ExperiencePerEssence = 20m;
    private const decimal EssencePerLap = 63m;

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "solo-aether-runes",
            "Solo aether runes",
            [
                .. RunecraftGlobal.CreateBaseBands(),
                .. SoloMudRunes.CreateMethodBands(settings)
                    .Where(band => band.StartExperience < 5_346_332),
                CreateBand(5_346_332, 99_000m, 2m, 0.125m, settings),
                CreateBand(RunecraftGlobal.RunecraftCapeExperience, 102_000m, 2m, 0m, settings)
            ],
            RunecraftGlobal.Note +
            " Requires level 90 Runecraft and assumes a colossal pouch, POH fairy ring, Construction cape, and Castle Wars banking. Rings of dueling are priced; untradeable teleport unlocks are excluded.");

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        decimal magicImbueAstralRunesPerLap,
        decimal pouchRepairsPerLap,
        RunecraftGlobal.RunecraftSettings settings)
    {
        var experiencePerLap = EssencePerLap * ExperiencePerEssence;
        var outputPerLap = RunecraftGlobal.OutputPerLap(EssencePerLap, settings);
        var resources = RunecraftGlobal.CreateCommonResources(
            experiencePerLap,
            EssencePerLap,
            0.2m,
            magicImbueAstralRunesPerLap,
            pouchRepairsPerLap);
        resources.Add(Input(Items.SoulRune, EssencePerLap / experiencePerLap));
        resources.Add(Input(Items.AetherCatalyst, outputPerLap / experiencePerLap));
        resources.Add(Input(Items.RingOfDueling8, 0.125m / experiencePerLap));
        resources.Add(Output(Items.AetherRune, outputPerLap / experiencePerLap));
        return Band(
            startExperience,
            experiencePerHour,
            "Solo aether runes",
            new TrainingEconomics(resources));
    }

    private static class Items
    {
        public static readonly CatalogueItem SoulRune = new(566, "Soul rune");
        public static readonly CatalogueItem AetherCatalyst = new(30771, "Aether catalyst");
        public static readonly CatalogueItem RingOfDueling8 = new(2552, "Ring of dueling(8)");
        public static readonly CatalogueItem AetherRune = new(30843, "Aether rune");
    }
}
