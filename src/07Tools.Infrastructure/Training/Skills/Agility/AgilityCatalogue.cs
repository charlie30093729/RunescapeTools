using RunescapeTools.Core.Training;
using RunescapeTools.Infrastructure.Training.Skills.Agility.Methods;

namespace RunescapeTools.Infrastructure.Training.Skills;

internal static class AgilityCatalogue
{
    public static TrainingSkillDefinition Create()
    {
        var main = HallowedSepulchre.Create();
        return new("Agility", main.Bands, Note: main.Note,
            Methods: [main, PrifddinasCourse.Create(), ArdougneCourse.Create()],
            DefaultMethodId: main.Id);
    }
}
