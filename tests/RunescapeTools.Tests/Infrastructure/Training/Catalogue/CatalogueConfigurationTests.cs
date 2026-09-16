namespace RunescapeTools.Tests.Infrastructure.Training.Catalogue;

[TestFixture]
[Category("Unit")]
public sealed class CatalogueConfigurationTests
{
    [Test]
    [Property("LegacyScenario", "TrainingSkillConfiguration")]
    public void ExpectedSkillsExposeConfigurators()
    {
        var catalogue = new MainEhpCatalogue();

        var configuredSkills = catalogue.Skills
            .Where(skill => skill.Configurator is not null)
            .Select(skill => skill.Skill)
            .ToArray();

        Assert.That(string.Join('|', configuredSkills), Is.EqualTo("Prayer|Fletching|Firemaking|Smithing|Mining|Herblore|Farming|Runecraft|Construction|Sailing"), "requested skill configurators");

        Assert.That(catalogue.Skills.Single(skill => skill.Skill == "Farming")
                .Configurator!.Definition.Options.Count, Is.EqualTo(0), "Farming placeholder configuration");
    }
}
