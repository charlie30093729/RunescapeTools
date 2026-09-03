using RunescapeTools.Core.Training;
namespace RunescapeTools.Infrastructure.Training.Skills.Runecraft.Methods;

internal static class DoloAetherRunes
{
    private const long UnlockExperience = 5_346_332;
    private const decimal ExperiencePerHour = 138_000m;
    private const string Name = "Dolo aether runes (1+1)";

    public static TrainingMethodDefinition Create(RunecraftGlobal.RunecraftSettings settings)
    {
        var solo = SoloAetherRunes.Create(settings);
        var bands = solo.Bands
            .Select(band => band.StartExperience >= UnlockExperience
                ? band with
                {
                    ExperiencePerHour = ExperiencePerHour,
                    Method = Name
                }
                : band)
            .ToArray();

        return new TrainingMethodDefinition(
            "dolo-aether-runes",
            Name,
            bands,
            RunecraftGlobal.Note +
            " The reviewed 138,000 XP/hour rate assumes one runner. All item quantities, GP/XP, and " +
            "configuration effects intentionally match Solo aether runes; additional runner-supplied " +
            "essence and every runner operating cost are excluded. GP/hour therefore scales from the " +
            "solo economics solely through the increased XP/hour rate.");
    }
}
