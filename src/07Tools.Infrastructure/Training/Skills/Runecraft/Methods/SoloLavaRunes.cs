using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class SoloLavaRunes
{
    private const decimal ExperiencePerEssence = 10.5m;

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "solo-lava-runes",
            "Solo lava runes",
            [
                .. RunecraftGlobal.CreateBaseBands().Where(band => band.StartExperience < 6_291),
                CreateBand(6_291, 40_000m, 28m, 0.125m, 2m, 0m, settings),
                CreateBand(101_333, 60_000m, 43m, 0.125m, 2m, 0.0345m, settings),
                CreateBand(1_210_421, 70_000m, 50m, 0.2m, 2m, 0.1m, settings),
                CreateBand(3_258_594, 102_100m, 63m, 0.2m, 2m, 0.125m, settings),
                CreateBand(
                    RunecraftGlobal.RunecraftCapeExperience,
                    102_100m,
                    63m,
                    0.2m,
                    2m,
                    0m,
                    settings)
            ],
            RunecraftGlobal.Note +
            " Lava rate bands use reviewed pouch breakpoints: level 23 entry, large pouch at 50, giant pouch at 75, and colossal pouch at 85. Ring of the elements is treated as reusable gear.");

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        decimal essencePerLap,
        decimal bindingNecklacesPerLap,
        decimal magicImbueAstralRunesPerLap,
        decimal pouchRepairsPerLap,
        RunecraftGlobal.RunecraftSettings settings)
    {
        var experiencePerLap = essencePerLap * ExperiencePerEssence;
        var resources = RunecraftGlobal.CreateCommonResources(
            experiencePerLap,
            essencePerLap,
            bindingNecklacesPerLap,
            magicImbueAstralRunesPerLap,
            pouchRepairsPerLap);
        resources.Add(Input(Items.EarthRune, essencePerLap / experiencePerLap));
        resources.Add(Output(
            Items.LavaRune,
            RunecraftGlobal.OutputPerLap(essencePerLap, settings) / experiencePerLap));
        return Band(
            startExperience,
            experiencePerHour,
            "Solo lava runes",
            new TrainingEconomics(resources));
    }

    private static class Items
    {
        public static readonly CatalogueItem EarthRune = new(557, "Earth rune");
        public static readonly CatalogueItem LavaRune = new(4699, "Lava rune");
    }
}
