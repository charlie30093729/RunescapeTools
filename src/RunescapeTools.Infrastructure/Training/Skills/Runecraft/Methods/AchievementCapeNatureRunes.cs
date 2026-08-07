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
                CreateNatureBand(settings)
            ],
            RunecraftGlobal.Note +
            " Requires level 91 Runecraft and assumes 120 laps per hour using the Achievement Diary " +
            "cape teleport, Shilo Village shortcut, colossal pouch, and Jarr banking. NPC Contact is " +
            "priced once every eight laps for pouch repair. The Desert amulet restoration and reusable " +
            "or untradeable equipment are excluded.");

    private static TrainingRateBand CreateNatureBand(RunecraftGlobal.RunecraftSettings settings)
    {
        var experiencePerLap = EssencePerLap * ExperiencePerEssence;
        var resources = RunecraftGlobal.CreateCommonResources(
            experiencePerLap,
            EssencePerLap,
            bindingNecklacesPerLap: 0m,
            astralRunesPerLap: NpcContactsPerLap,
            airRunesPerLap: NpcContactsPerLap * 2m,
            cosmicRunesPerLap: NpcContactsPerLap);
        resources.Add(Output(
            Items.NatureRune,
            RunecraftGlobal.OutputPerLap(BaseNatureRunesPerLap, settings) / experiencePerLap));

        return Band(
            UnlockExperience,
            69_120m,
            "Double nature runes - Achievement Diary cape",
            new TrainingEconomics(resources));
    }

    private static class Items
    {
        public static readonly CatalogueItem NatureRune = new(561, "Nature rune");
    }
}
