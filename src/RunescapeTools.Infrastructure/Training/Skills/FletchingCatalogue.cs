using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class FletchingCatalogue
{
    private const decimal AmethystDartFletchingXp = 21m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Fletching",
            Band(0, 1_000_000m, "Zero-time Fletching - rate only"),
            Band(
                5_346_332,
                1_000_000m,
                "Amethyst darts",
                new TrainingEconomics(
                    [
                        Input(AmethystDartTip, "Amethyst dart tip", 1m / AmethystDartFletchingXp),
                        Input(Feather, "Feather", 1m / AmethystDartFletchingXp),
                        Output(AmethystDart, "Amethyst dart", 1m / AmethystDartFletchingXp)
                    ])));
}
