using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Fletching;

internal static class FletchingCatalogue
{
    private const decimal AmethystDartFletchingXp = 21m;

    public static TrainingSkillDefinition Create()
    {
        var method = new TrainingMethodDefinition(
            "main-ehp",
            "Amethyst darts",
            [
                Band(0, 1_000_000m, "Zero-time Fletching - rate only"),
                Band(
                    5_346_332,
                    1_000_000m,
                    "Amethyst darts",
                    new TrainingEconomics(
                    [
                        Input(Items.AmethystDartTip, 1m / AmethystDartFletchingXp),
                        Input(Items.Feather, 1m / AmethystDartFletchingXp),
                        Output(Items.AmethystDart, 1m / AmethystDartFletchingXp)
                    ]))
            ]);
        return new TrainingSkillDefinition(
            "Fletching",
            method.Bands,
            Methods: [method],
            DefaultMethodId: method.Id,
            Configurator: FletchingGlobal.Configurator);
    }

    private static class Items
    {
        public static readonly CatalogueItem AmethystDartTip = new(25853, "Amethyst dart tip");
        public static readonly CatalogueItem Feather = new(314, "Feather");
        public static readonly CatalogueItem AmethystDart = new(25849, "Amethyst dart");
    }
}
