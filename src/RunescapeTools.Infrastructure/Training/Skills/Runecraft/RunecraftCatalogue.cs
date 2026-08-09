using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft;

internal static class RunecraftCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var settings = RunecraftGlobal.ResolveSettings();
        var defaultMethod = SoloMudRunes.Create(settings);

        return new TrainingSkillDefinition(
            "Runecraft",
            defaultMethod.Bands,
            Note: RunecraftGlobal.Note,
            Methods:
            [
                defaultMethod,
                SoloLavaRunes.Create(settings),
                SoloAetherRunes.Create(settings),
                AchievementCapeNatureRunes.Create(settings),
                ArceuusBloodRunes.Create(settings),
                ArceuusSoulRunes.Create(settings)
            ],
            DefaultMethodId: defaultMethod.Id,
            Configurator: RunecraftGlobal.Configurator);
    }
}
