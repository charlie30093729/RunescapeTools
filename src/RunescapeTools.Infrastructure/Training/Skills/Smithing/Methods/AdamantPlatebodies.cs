using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing.Methods;

internal static class AdamantPlatebodies
{
    private const decimal ExperiencePerPlatebody = 312.5m;
    private const decimal BaseExperiencePerHour = 260_400m;
    private const decimal UniformExperiencePerHour = 325_000m;

    public static TrainingMethodDefinition Create(bool smithsUniform)
    {
        var rate = smithsUniform
            ? UniformExperiencePerHour
            : BaseExperiencePerHour;
        var band = Band(
            4_382_299,
            rate,
            "Adamant platebodies",
            new TrainingEconomics(
            [
                Input(Items.AdamantiteBar, 5m / ExperiencePerPlatebody),
                Output(Items.AdamantPlatebody, 1m / ExperiencePerPlatebody)
            ]));

        return new TrainingMethodDefinition(
            "adamant-platebodies",
            "Adamant platebodies",
            SmithingGlobal.CreateRoute(band),
            "Requires level 88 Smithing. The saved full-outfit rate is 325,000 XP/hour; without it the reviewed baseline is 260,400 XP/hour.");
    }

    private static class Items
    {
        public static readonly CatalogueItem AdamantiteBar = new(2361, "Adamantite bar");
        public static readonly CatalogueItem AdamantPlatebody = new(1123, "Adamant platebody");
    }
}
