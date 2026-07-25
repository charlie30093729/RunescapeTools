using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

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
            Input(PureEssence, "Pure essence", essencePerLap / experiencePerLap),
            Input(EarthRune, "Earth rune", essencePerLap / experiencePerLap),
            Input(BindingNecklace, "Binding necklace", 0.2m / experiencePerLap),
            Input(AstralRune, "Astral rune", astralRunesPerLap / experiencePerLap),
            Output(MudRune, "Mud rune", mudRunesPerLap / experiencePerLap)
        };
        if (airRunesPerLap > 0m)
            resources.Add(Input(AirRune, "Air rune", airRunesPerLap / experiencePerLap));
        if (cosmicRunesPerLap > 0m)
            resources.Add(Input(CosmicRune, "Cosmic rune", cosmicRunesPerLap / experiencePerLap));
        return new TrainingEconomics(resources);
    }
}
