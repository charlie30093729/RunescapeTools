using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Prayer;

internal static class PrayerCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var method = PrayerGlobal.CreateMethod(
            PrayerGlobal.ResolveSettings());
        return new TrainingSkillDefinition(
            "Prayer",
            method.Bands,
            Note: method.Note,
            Methods: [method],
            DefaultMethodId: method.Id,
            Configurator: PrayerGlobal.Configurator);
    }
}
