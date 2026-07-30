using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing;

internal static class SmithingCatalogue
{
    private const decimal GoldBarSmithingXp = 56.2m;
    private const decimal BlastFurnaceGpPerHour = 72_000m;
    private const decimal BlastFurnaceUnder60GpPerHour = 87_000m;
    private const decimal StaminaPotion4PerHour = 10m;

    public static TrainingSkillDefinition Create() =>
        new(
            "Smithing",
            [
                Band(0, 46_500m, "Quests"),
                Band(37_224, 380_000m, "Solo Blast Furnace gold", GoldEconomics(BlastFurnaceUnder60GpPerHour)),
                Band(273_742, 380_000m, "Solo Blast Furnace gold", GoldEconomics(BlastFurnaceGpPerHour)),
                Band(13_034_431, 410_000m, "Solo Blast Furnace gold", GoldEconomics(BlastFurnaceGpPerHour))
            ],
            Note: "Smiths' uniform is saved for future anvil methods and does not affect the current Blast Furnace route.",
            Configurator: SmithingGlobal.Configurator);

    private static TrainingEconomics GoldEconomics(decimal fixedGpPerHour) =>
        new(
            [
                Input(Items.GoldOre, 1m / GoldBarSmithingXp),
                Input(Items.StaminaPotion4, 0m, quantityPerHour: StaminaPotion4PerHour),
                Output(Items.GoldBar, 1m / GoldBarSmithingXp)
            ],
            FixedGpPerHour: fixedGpPerHour);

    private static class Items
    {
        public static readonly CatalogueItem GoldOre = new(444, "Gold ore");
        public static readonly CatalogueItem StaminaPotion4 = new(12625, "Stamina potion(4)");
        public static readonly CatalogueItem GoldBar = new(2357, "Gold bar");
    }
}
