namespace RunescapeTools.Tests.Core.Training;

[TestFixture]
[Category("Unit")]
public sealed class SecondaryExperienceRoundingTests
{
    [Test]
    public void RepeatingRatioDoesNotLoseAWholeExperiencePointAtTheBoundary()
    {
        var definition = new TrainingSkillDefinition("Prayer",
            [new TrainingRateBand(0, 720_000m, "Test offering", ExperienceOutputs:
                [new TrainingExperienceFlow("Agility", 1_340.6m / 7_200m)])]);
        var result = new TrainingPlanCalculator().Calculate(definition, 0, 720_000,
            new Dictionary<int, ItemPrice>());
        Assert.That(result.GeneratedExperience["Agility"], Is.EqualTo(134_060m));
    }

    [Test]
    public void GenuineFractionalExperienceIsNotRoundedUpToWholeCredit()
    {
        var definition = new TrainingSkillDefinition("Prayer",
            [new TrainingRateBand(0, 100m, "Test offering", ExperienceOutputs:
                [new TrainingExperienceFlow("Agility", 0.75m)])]);
        var result = new TrainingPlanCalculator().Calculate(definition, 0, 1,
            new Dictionary<int, ItemPrice>());
        Assert.That(result.GeneratedExperience["Agility"], Is.EqualTo(0.75m));
    }
}
