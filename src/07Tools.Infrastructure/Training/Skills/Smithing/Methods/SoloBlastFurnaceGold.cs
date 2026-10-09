using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Smithing.Methods;

internal static class SoloBlastFurnaceGold
{
    private const decimal GoldBarSmithingXp = 56.2m;
    private const decimal StaminaPotion4PerHour = 10m;

    public static TrainingMethodDefinition Create() =>
        new(
            "main-ehp",
            "Solo Blast Furnace gold",
            SmithingGlobal.CreateRoute(
                Band(13_034_431, 410_000m, "Solo Blast Furnace gold", GoldEconomics(72_000m))));

    internal static TrainingEconomics GoldEconomics(decimal fixedGpPerHour) =>
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
