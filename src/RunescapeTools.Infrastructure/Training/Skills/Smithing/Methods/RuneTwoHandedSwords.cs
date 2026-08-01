using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing.Methods;

internal static class RuneTwoHandedSwords
{
    private const decimal ExperiencePerSword = 225m;
    private const decimal BaseExperiencePerHour = 217_000m;
    private const decimal StandardCycleTicks = 56m;
    private const decimal UniformCycleTicks = 47m;

    public static TrainingMethodDefinition Create(bool smithsUniform)
    {
        var rate = smithsUniform
            ? BaseExperiencePerHour * StandardCycleTicks / UniformCycleTicks
            : BaseExperiencePerHour;
        var band = Band(
            13_034_431,
            rate,
            "Rune 2h swords",
            new TrainingEconomics(
            [
                Input(Items.RuniteBar, 3m / ExperiencePerSword),
                Output(Items.Rune2hSword, 1m / ExperiencePerSword)
            ]));

        return new TrainingMethodDefinition(
            "rune-2h-swords",
            "Rune 2h swords",
            SmithingGlobal.CreateRoute(band),
            "Requires level 99 Smithing. The full Smiths' uniform reduces each anvil action from five ticks to four; the rate retains the reviewed 11-tick bank cycle.");
    }

    private static class Items
    {
        public static readonly CatalogueItem RuniteBar = new(2363, "Runite bar");
        public static readonly CatalogueItem Rune2hSword = new(1319, "Rune 2h sword");
    }
}
