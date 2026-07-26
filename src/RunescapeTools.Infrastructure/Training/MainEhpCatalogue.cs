using RunescapeTools.Application.Training;
using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills;

namespace RunescapeTools.Infrastructure.Training;

public sealed class MainEhpCatalogue : IEhpCatalogue
{
    public string Version => "OSRS training catalogue 2026-07";

    public DateOnly VerifiedOn => new(2026, 7, 26);

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
