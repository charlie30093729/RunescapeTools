using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class DoloAetherRunes
{
    private const decimal ExperiencePerEssence = 20m;
    private const decimal MainEssencePerLap = 63m;
    private const decimal RunnerEssencePerLap = 46m;
    private const decimal EssencePerLap = MainEssencePerLap + RunnerEssencePerLap;
    private const decimal ExperiencePerLap = EssencePerLap * ExperiencePerEssence;
    private const decimal BindingNecklacesPerLap = 1m / 3m;

    private static readonly decimal[] CraftBatchSizes = [23m, 23m, 17m, 23m, 23m];

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings) =>
        new(
            "dolo-aether-runes",
            "Dolo aether runes (1+1)",
            [
                .. RunecraftGlobal.CreateBaseBands(),
                .. SoloMudRunes.CreateMethodBands(settings)
                    .Where(band => band.StartExperience < 5_346_332),
                CreateBand(5_346_332, 138_000m, 0.125m, settings),
                CreateBand(RunecraftGlobal.RunecraftCapeExperience, 138_000m, 0m, settings)
            ],
            RunecraftGlobal.Note +
            " Requires level 90 Runecraft on the crafting account and a level-75 runner with a colossal pouch. " +
            "The runner carries 23 loose essence and deliberately underfills the pouch to 23 for two equal " +
            "trades; together with the main account's 63 essence, five altar crafts award 2,180 XP per lap. " +
            "The reviewed 138,000 XP/hour rate includes controlling both clients. Economics price two Magic " +
            "Imbue casts, one binding necklace every three laps after discarding its final charge, both " +
            "accounts' ring charges, and only the main account's pre-99 pouch repairs. The runner is assumed " +
            "to use a redwood-lit Abyssal lantern. Daeyalt essence is excluded because it cannot be traded.");

    private static TrainingRateBand CreateBand(
        long startExperience,
        decimal experiencePerHour,
        decimal pouchRepairsPerLap,
        RunecraftGlobal.RunecraftSettings settings)
    {
        var outputPerLap = CraftBatchSizes.Sum(batch =>
            RunecraftGlobal.OutputPerLap(batch, settings));
        var resources = RunecraftGlobal.CreateCommonResources(
            ExperiencePerLap,
            EssencePerLap,
            BindingNecklacesPerLap,
            4m,
            pouchRepairsPerLap);
        resources.Add(Input(Items.SoulRune, EssencePerLap / ExperiencePerLap));
        resources.Add(Input(Items.AetherCatalyst, outputPerLap / ExperiencePerLap));
        resources.Add(Input(Items.RingOfDueling8, 0.25m / ExperiencePerLap));
        resources.Add(Output(Items.AetherRune, outputPerLap / ExperiencePerLap));
        return Band(
            startExperience,
            experiencePerHour,
            "Dolo aether runes (1+1)",
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
