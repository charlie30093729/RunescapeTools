using RunescapeTools.Application.Training;
using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills;
using RunescapeTools.Infrastructure.Training.Skills.Construction;
using RunescapeTools.Infrastructure.Training.Skills.Crafting;
using RunescapeTools.Infrastructure.Training.Skills.Farming;
using RunescapeTools.Infrastructure.Training.Skills.Firemaking;
using RunescapeTools.Infrastructure.Training.Skills.Fletching;
using RunescapeTools.Infrastructure.Training.Skills.Herblore;
using RunescapeTools.Infrastructure.Training.Skills.Hunter;
using RunescapeTools.Infrastructure.Training.Skills.Prayer;
using RunescapeTools.Infrastructure.Training.Skills.Runecraft;
using RunescapeTools.Infrastructure.Training.Skills.Smithing;

namespace RunescapeTools.Infrastructure.Training;

public sealed class MainEhpCatalogue : IEhpCatalogue
{
    public string Version => "OSRS training catalogue 2026-08";

    public DateOnly VerifiedOn => new(2026, 8, 1);

    public IReadOnlyList<TrainingSkillDefinition> Skills { get; } =
    [
        DefenceCatalogue.Create(),
        RangedCatalogue.Create(),
        PrayerCatalogue.Create(),
        MagicCatalogue.Create(),
        CookingCatalogue.Create(),
        WoodcuttingCatalogue.Create(),
        FletchingCatalogue.Create(),
        FishingCatalogue.Create(),
        FiremakingCatalogue.Create(),
        CraftingCatalogue.Create(),
        SmithingCatalogue.Create(),
        MiningCatalogue.Create(),
        HerbloreCatalogue.Create(),
        AgilityCatalogue.Create(),
        ThievingCatalogue.Create(),
        SlayerCatalogue.Create(),
        FarmingCatalogue.Create(),
        RunecraftCatalogue.Create(),
        HunterCatalogue.Create(),
        ConstructionCatalogue.Create(),
        SailingCatalogue.Create()
    ];
}
