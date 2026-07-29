using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class RunecraftCatalogue
{
    public static TrainingSkillDefinition Create() =>
        Skill(
            "Runecraft",
            "Solo mud-rune flows include Magic Imbue, pouch repair below 99, and discarded binding-necklace charges; reusable gear is excluded.",
            Band(0, 13_600m, "Quests"),
            Band(33_210, 45_000m, "Guardians of the Rift rewards"),
            Band(1_210_421, 74_500m, "Solo mud runes", SoloMudRuneEconomicsLevel75()),
            Band(3_258_594, 96_900m, "Solo mud runes", SoloMudRuneEconomicsLevel85()),
            Band(13_034_431, 98_200m, "Solo mud runes", SoloMudRuneEconomicsLevel99()));

    private static TrainingEconomics SoloMudRuneEconomicsLevel75() =>
        SoloMudRuneEconomics(
            experiencePerLap: 475m,
            essencePerLap: 50m,
            mudRunesPerLap: 74m,
            astralRunesPerLap: 2.1m,
            airRunesPerLap: 0.2m,
            cosmicRunesPerLap: 0.1m);

    private static TrainingEconomics SoloMudRuneEconomicsLevel85() =>
        SoloMudRuneEconomics(
            experiencePerLap: 598.5m,
            essencePerLap: 63m,
            mudRunesPerLap: 93m,
            astralRunesPerLap: 2.125m,
            airRunesPerLap: 0.25m,
            cosmicRunesPerLap: 0.125m);

    private static TrainingEconomics SoloMudRuneEconomicsLevel99() =>
        SoloMudRuneEconomics(
            experiencePerLap: 598.5m,
            essencePerLap: 63m,
            mudRunesPerLap: 93m,
            astralRunesPerLap: 2m,
            airRunesPerLap: 0m,
            cosmicRunesPerLap: 0m);

    private static TrainingEconomics SoloMudRuneEconomics(
        decimal experiencePerLap,
        decimal essencePerLap,
        decimal mudRunesPerLap,
        decimal astralRunesPerLap,
        decimal airRunesPerLap,
        decimal cosmicRunesPerLap)
    {
        var resources = new List<TrainingResourceFlow>
        {
            Input(Items.PureEssence, essencePerLap / experiencePerLap),
            Input(Items.EarthRune, essencePerLap / experiencePerLap),
            Input(Items.BindingNecklace, 0.2m / experiencePerLap),
            Input(Items.AstralRune, astralRunesPerLap / experiencePerLap),
            Output(Items.MudRune, mudRunesPerLap / experiencePerLap)
        };
        if (airRunesPerLap > 0m)
            resources.Add(Input(Items.AirRune, airRunesPerLap / experiencePerLap));
        if (cosmicRunesPerLap > 0m)
            resources.Add(Input(Items.CosmicRune, cosmicRunesPerLap / experiencePerLap));
        return new TrainingEconomics(resources);
    }

    private static class Items
    {
        public static readonly CatalogueItem PureEssence = new(7936, "Pure essence");
        public static readonly CatalogueItem EarthRune = new(557, "Earth rune");
        public static readonly CatalogueItem BindingNecklace = new(5521, "Binding necklace");
        public static readonly CatalogueItem AstralRune = new(9075, "Astral rune");
        public static readonly CatalogueItem MudRune = new(4698, "Mud rune");
        public static readonly CatalogueItem AirRune = new(556, "Air rune");
        public static readonly CatalogueItem CosmicRune = new(564, "Cosmic rune");
    }
}
