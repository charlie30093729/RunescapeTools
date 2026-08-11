using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class AchievementCapeNatureRunes
{
    private const long UnlockExperience = 5_902_831;
    private const decimal ExperiencePerEssence = 9m;
    private const decimal EssencePerLap = 64m;
    private const decimal BaseNatureRunesPerLap = EssencePerLap * 2m;
    private const decimal NpcContactsPerLap = 0.125m;

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "achievement-cape-double-nature-runes",
            "Double nature runes - Achievement Diary cape",
            [
                .. RunecraftGlobal.CreateBaseBands(),
                .. SoloMudRunes.CreateMethodBands(settings)
                    .Where(band => band.StartExperience < UnlockExperience),
                CreateNatureBand(UnlockExperience, NpcContactsPerLap, settings),
                CreateNatureBand(RunecraftGlobal.RunecraftCapeExperience, 0m, settings)
            ],
            RunecraftGlobal.Note +
            " Requires level 91 Runecraft and assumes 120 laps per hour using the Achievement Diary " +
            "cape teleport, Shilo Village shortcut, colossal pouch, and Jarr banking. NPC Contact is " +
            "priced once every eight laps before level 99; the Runecraft cape prevents further pouch " +
            "degradation from level 99 onward. The Desert amulet restoration and reusable " +
            "or untradeable equipment are excluded.");

    private static TrainingRateBand CreateNatureBand(
        long startExperience,
        decimal pouchRepairsPerLap,
        RunecraftGlobal.RunecraftSettings settings)
    {
        var experiencePerLap = EssencePerLap * ExperiencePerEssence;
        var resources = RunecraftGlobal.CreateCommonResources(
            experiencePerLap,
            EssencePerLap,
            bindingNecklacesPerLap: 0m,
            magicImbueAstralRunesPerLap: 0m,
            pouchRepairsPerLap: pouchRepairsPerLap);
        resources.Add(Output(
            Items.NatureRune,
            RunecraftGlobal.OutputPerLap(BaseNatureRunesPerLap, settings) / experiencePerLap));

        return Band(
            startExperience,
            69_120m,
            "Double nature runes - Achievement Diary cape",
            new TrainingEconomics(resources));
    }

    private static class Items
    {
        public static readonly CatalogueItem NatureRune = new(561, "Nature rune");
    }
}
