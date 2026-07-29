using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class SmithingCatalogue
{
    private const decimal GoldBarSmithingXp = 56.2m;
    private const decimal BlastFurnaceGpPerHour = 72_000m;
    private const decimal BlastFurnaceUnder60GpPerHour = 87_000m;
    private const decimal StaminaPotion4PerHour = 10m;

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Smithing",
            Band(0, 46_500m, "Quests"),
            Band(37_224, 380_000m, "Solo Blast Furnace gold", GoldEconomics(BlastFurnaceUnder60GpPerHour)),
            Band(273_742, 380_000m, "Solo Blast Furnace gold", GoldEconomics(BlastFurnaceGpPerHour)),
            Band(13_034_431, 410_000m, "Solo Blast Furnace gold", GoldEconomics(BlastFurnaceGpPerHour)));

    private static TrainingEconomics GoldEconomics(decimal fixedGpPerHour) =>
        new(
            [
                Input(GoldOre, "Gold ore", 1m / GoldBarSmithingXp),
                Input(StaminaPotion4, "Stamina potion(4)", 0m, quantityPerHour: StaminaPotion4PerHour),
                Output(GoldBar, "Gold bar", 1m / GoldBarSmithingXp)
            ],
            FixedGpPerHour: fixedGpPerHour);
}
