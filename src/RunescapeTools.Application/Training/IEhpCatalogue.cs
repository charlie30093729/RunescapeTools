using RunescapeTools.Core.Training;

namespace RunescapeTools.Application.Training;

public interface IEhpCatalogue
{
    string Version { get; }

    DateOnly VerifiedOn { get; }

    IReadOnlyList<TrainingSkillDefinition> Skills { get; }
}
