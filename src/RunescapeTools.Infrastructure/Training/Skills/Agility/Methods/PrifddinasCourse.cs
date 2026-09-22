using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Agility.Methods;

internal static class PrifddinasCourse
{
    private const string Name = "Prifddinas Agility Course";
    private const decimal ExperiencePerLap = 1_340.6m;
    private const decimal ShardsPerLap = 0.94m;
    private const decimal DivinePotionsPerShard = 2.5m;

    public static TrainingMethodDefinition Create() => new(
        "prifddinas-agility-course", Name,
        AgilityGlobal.WithMainRouteBeforeUnlock(
            CreateBand(AgilityGlobal.Level75Experience, 54_000m),
            CreateBand(AgilityGlobal.Level80Experience, 58_000m),
            CreateBand(AgilityGlobal.Level85Experience, 62_000m),
            CreateBand(AgilityGlobal.Level90Experience, 66_000m)),
        "Standalone Agility only: requires level 75 Agility and Song of the Elves. " +
        "Uses the Wiki course table's portal-assisted 54k/58k/62k/66k XP per hour at " +
        "levels 75/80/85/90; estimates are held until the next band. The Wiki prose also " +
        "quotes approximately 65k at 90. Actual rates depend on attention and failures " +
        "(failproof at 91). Expected rewards use 1,340.6 XP and 0.94 crystal shards per lap. " +
        "All shards are converted to divine super combat potion(4) at 2.5 potions per shard, " +
        "requiring 97 Herblore; buy the regular potion high and sell the divine potion low " +
        "after GE tax. Conversion time, Herblore XP, travel and optional food/energy supplies " +
        "are excluded. No offering spell, bones, ashes or Magic XP are included. Prayer-at-Prif " +
        "keeps its separate calculation; only remaining Agility XP is trained here after credits. " +
        "The existing legacy route is retained below level 75; its Floor 5 assumption is not " +
        "a verified lower-level route. Quest and Herblore requirements are disclosed, not profile-checked.",
        UseStableDisplayName: true);

    private static TrainingRateBand CreateBand(long startExperience, decimal rate)
    {
        const decimal shardsPerExperience = ShardsPerLap / ExperiencePerLap;
        return Band(startExperience, rate, Name, new TrainingEconomics(
        [
            new(Items.CrystalShard.Id, Items.CrystalShard.Name, shardsPerExperience,
                TrainingFlowDirection.Output, SubjectToGeTax: false, RequiresMarketPrice: false),
            Input(Items.SuperCombatPotion4, shardsPerExperience * DivinePotionsPerShard),
            Output(Items.DivineSuperCombatPotion4, shardsPerExperience * DivinePotionsPerShard)
        ]));
    }

    private static class Items
    {
        public static readonly CatalogueItem CrystalShard = new(23962, "Crystal shard");
        public static readonly CatalogueItem SuperCombatPotion4 = new(12695, "Super combat potion(4)");
        public static readonly CatalogueItem DivineSuperCombatPotion4 = new(23685, "Divine super combat potion(4)");
    }
}
