using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class SoloMudRunes
{
    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "main-ehp",
            "Solo mud runes",
            [
                .. RunecraftGlobal.CreateBaseBands(),
                .. CreateMethodBands(settings)
            ],
            RunecraftGlobal.Note);

    internal static IReadOnlyList<TrainingRateBand> CreateMethodBands(
        RunecraftGlobal.RunecraftSettings settings) =>
    [
        CreateBand(1_210_421, 74_500m, 475m, 50m, 2m, 0.1m, settings),
        CreateBand(3_258_594, 96_900m, 598.5m, 63m, 2m, 0.125m, settings),
        CreateBand(RunecraftGlobal.RunecraftCapeExperience, 98_200m, 598.5m, 63m, 2m, 0m, settings)
    ];

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        decimal experiencePerLap,
        decimal essencePerLap,
        decimal magicImbueAstralRunesPerLap,
        decimal pouchRepairsPerLap,
        RunecraftGlobal.RunecraftSettings settings)
    {
        var resources = RunecraftGlobal.CreateCommonResources(
            experiencePerLap,
            essencePerLap,
            0.2m,
            magicImbueAstralRunesPerLap,
            pouchRepairsPerLap);
        resources.Add(Input(Items.EarthRune, essencePerLap / experiencePerLap));
        resources.Add(Output(
            Items.MudRune,
            RunecraftGlobal.OutputPerLap(essencePerLap, settings) / experiencePerLap));
        return Band(
            startExperience,
            experiencePerHour,
            "Solo mud runes",
            new TrainingEconomics(resources));
    }

    private static class Items
    {
        public static readonly CatalogueItem EarthRune = new(557, "Earth rune");
        public static readonly CatalogueItem MudRune = new(4698, "Mud rune");
    }
}
