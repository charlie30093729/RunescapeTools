using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Prayer.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer;

internal static class PrayerCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var settings = PrayerGlobal.ResolveSettings();
        var method = SuperiorDragonBones.Create(settings);
        return new TrainingSkillDefinition(
            "Prayer",
            method.Bands,
            Note: method.Note,
            Methods:
            [
                method,
                DragonBones.Create(settings)
            ],
            DefaultMethodId: method.Id,
            Configurator: PrayerGlobal.Configurator);
    }
}
