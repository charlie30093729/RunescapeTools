using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Fishing.Methods;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Fishing;

internal static class FishingGlobal
{
    public const long BarbarianFishingUnlockExperience = 83_014;

    public static IReadOnlyList<TrainingRateBand> CreateBarbarianBands(
        string method,
        decimal level48Rate,
        decimal level58Rate,
        decimal level70Rate,
        decimal level80Rate,
        decimal level90Rate,
        decimal level99Rate) =>
    [
        CreateBand(BarbarianFishingUnlockExperience, level48Rate, method, 10m),
        CreateBand(224_466, level58Rate, method, 10.8m),
        CreateBand(737_627, level70Rate, method, 10.9m),
        CreateBand(1_986_068, level80Rate, method, 11.1m),
        CreateBand(5_346_332, level90Rate, method, 11.3m),
        CreateBand(13_034_431, level99Rate, method, 11.4m)
    ];

    public static IReadOnlyList<TrainingRateBand> WithMainRouteBeforeUnlock(
        IReadOnlyList<TrainingRateBand> barbarianBands) =>
        MainEhp.Create().Bands
            .Where(band => band.StartExperience < BarbarianFishingUnlockExperience)
            .Concat(barbarianBands)
            .ToArray();

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        string method,
        decimal fishingPerAgilityExperience) =>
        Band(
            startExperience,
            experiencePerHour,
            method,
            experienceOutputs:
            [
                new TrainingExperienceFlow(
                    "Agility",
                    1m / fishingPerAgilityExperience)
            ]);
}
