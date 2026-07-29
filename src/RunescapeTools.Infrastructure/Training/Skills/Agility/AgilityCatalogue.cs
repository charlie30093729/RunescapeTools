using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;
using static RunescapeTools.Infrastructure.Training.TrainingItemIds;

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
                    StaminaPotion4,
                    "Stamina potion(4)",
                    StaminaPotionsPerCoffin / ExperiencePerGrandCoffin),
                Output(
                    RingOfEnduranceUncharged,
                    "Ring of endurance (uncharged)",
                    1m / 200m / ExperiencePerGrandCoffin),
                Output(Rune2hSword, "Rune 2h sword", 0.1m / ExperiencePerGrandCoffin),
                Output(RunePlatebody, "Rune platebody", 0.1m / ExperiencePerGrandCoffin),
                Output(LawRune, "Law rune", 20m / ExperiencePerGrandCoffin),
                Output(BloodRune, "Blood rune", 20m / ExperiencePerGrandCoffin),
                Output(SoulRune, "Soul rune", 20m / ExperiencePerGrandCoffin),
                Output(RuniteBolts, "Runite bolts", 20m / ExperiencePerGrandCoffin),
                Output(Monkfish, "Monkfish", 0.4m / ExperiencePerGrandCoffin),
                Output(SanfewSerum4, "Sanfew serum(4)", 0.15m / ExperiencePerGrandCoffin),
                Output(RanarrSeed, "Ranarr seed", 0.15m / ExperiencePerGrandCoffin)
            ],
            FixedGpOutputPerExperience: 2_125m / ExperiencePerGrandCoffin);
}
