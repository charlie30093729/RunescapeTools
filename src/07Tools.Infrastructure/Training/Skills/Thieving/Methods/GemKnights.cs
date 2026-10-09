using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Thieving.Methods;

internal static class GemKnights
{
    private const decimal ExperiencePerPickpocket = 103.4m;
    private const decimal RogueEquipmentExpectedYield = 1.8m;
    private const decimal TokkulDropChance = 182m / 195m;
    private const decimal AverageTokkulPerDrop = 5m;
    private const decimal TokkulPerDiscountedOnyx = 260_000m;

    public static TrainingMethodDefinition Create() => new(
        "main-ehp", "Gem knights",
        [Band(0, 260_000m, "Gem knights", new TrainingEconomics(
            [Output(Items.UncutOnyx,
                TokkulDropChance * AverageTokkulPerDrop * RogueEquipmentExpectedYield
                / TokkulPerDiscountedOnyx / ExperiencePerPickpocket)]))],
        "Reviewed level-99 planning projection: 260,000 XP/hour and 103.4 XP per successful TzHaar-Hur pickpocket. " +
        "Only Tokkul is valued: the official 182/195 chance at 3-7 Tokkul uses a five-Tokkul average and an expected " +
        "1.8x yield from four rogue pieces while ice gloves occupy the glove slot. Tokkul is converted to live-priced " +
        "uncut onyx at the Karamja-gloves rate of 260,000 Tokkul each. Gems, Rocky, variable healing/dodgy-necklace " +
        "usage, banking time, and unlock requirements are excluded.",
        UseStableDisplayName: true);

    private static class Items
    {
        public static readonly CatalogueItem UncutOnyx = new(6571, "Uncut onyx (Tokkul conversion)");
    }
}
