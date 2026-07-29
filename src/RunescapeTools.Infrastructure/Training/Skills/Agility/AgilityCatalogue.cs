using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class AgilityCatalogue
{
    private const decimal ExperiencePerGrandCoffin = 11_700m;
    private const decimal ReviewedExperiencePerHour = 98_500m;
    private const decimal ThievingExperiencePerCoffin = 200m;
    private const decimal StaminaPotionsPerCoffin = 0.5m;
    private static decimal ReviewedCoffinCount =>
        Math.Ceiling(TrainingPlanCalculator.MaximumExperience / ExperiencePerGrandCoffin);

    public static TrainingSkillDefinition Create() =>
        Skill(
            "Agility",
            Band(
                0,
                ReviewedExperiencePerHour,
                "Hallowed Sepulchre - Grand Coffin",
                GrandCoffinEconomics()),
            $"Reviewed 0-200m projection: approximately {ReviewedCoffinCount:N0} Floor 5 Grand Hallowed Coffins, " +
            $"{Math.Ceiling(ReviewedCoffinCount * StaminaPotionsPerCoffin):N0} stamina potion(4), and " +
            $"{ReviewedCoffinCount * ThievingExperiencePerCoffin:N0} incidental Thieving XP. Only the Grand Coffin is looted. " +
            "Stamina effects assume a charged Ring of endurance. Hallowed marks and elite clues are excluded from GP; " +
            "Floor 5 requires level 92 Agility.");

    private static TrainingEconomics GrandCoffinEconomics() =>
        new(
            [
                Input(
                    Items.StaminaPotion4,
                    StaminaPotionsPerCoffin / ExperiencePerGrandCoffin),
                Output(
                    Items.RingOfEnduranceUncharged,
                    1m / 200m / ExperiencePerGrandCoffin),
                Output(Items.Rune2hSword, 0.1m / ExperiencePerGrandCoffin),
                Output(Items.RunePlatebody, 0.1m / ExperiencePerGrandCoffin),
                Output(Items.LawRune, 20m / ExperiencePerGrandCoffin),
                Output(Items.BloodRune, 20m / ExperiencePerGrandCoffin),
                Output(Items.SoulRune, 20m / ExperiencePerGrandCoffin),
                Output(Items.RuniteBolts, 20m / ExperiencePerGrandCoffin),
                Output(Items.Monkfish, 0.4m / ExperiencePerGrandCoffin),
                Output(Items.SanfewSerum4, 0.15m / ExperiencePerGrandCoffin),
                Output(Items.RanarrSeed, 0.15m / ExperiencePerGrandCoffin)
            ],
            FixedGpOutputPerExperience: 2_125m / ExperiencePerGrandCoffin);

    private static class Items
    {
        public static readonly CatalogueItem StaminaPotion4 = new(12625, "Stamina potion(4)");
        public static readonly CatalogueItem RingOfEnduranceUncharged = new(24844, "Ring of endurance (uncharged)");
        public static readonly CatalogueItem Rune2hSword = new(1319, "Rune 2h sword");
        public static readonly CatalogueItem RunePlatebody = new(1127, "Rune platebody");
        public static readonly CatalogueItem LawRune = new(563, "Law rune");
        public static readonly CatalogueItem BloodRune = new(565, "Blood rune");
        public static readonly CatalogueItem SoulRune = new(566, "Soul rune");
        public static readonly CatalogueItem RuniteBolts = new(9144, "Runite bolts");
        public static readonly CatalogueItem Monkfish = new(7946, "Monkfish");
        public static readonly CatalogueItem SanfewSerum4 = new(10925, "Sanfew serum(4)");
        public static readonly CatalogueItem RanarrSeed = new(5295, "Ranarr seed");
    }
}
