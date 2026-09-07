namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Runecraft;

[TestFixture]
[Category("Unit")]
public sealed class OuraniaAltarTests
{
    [Test]
    [Property("LegacyScenario", "OuraniaAltarZmiMethod")]
    [Description("Ourania Altar exposes level-banded ZMI rates, outputs, and Daeyalt")]
    public void OuraniaAltarZmiMethod()
    {
        const decimal level99ExperiencePerEssence = 15.5788m;
        var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Runecraft");
        var method = definition.ResolveMethod("ourania-altar-zmi");

        Assert.That(method.Bands.Count, Is.EqualTo(14), "ZMI level and pouch bands");
        Assert.That(method.Bands.Single(band => band.StartExperience == 0).ExperiencePerHour, Is.EqualTo(20_423m).Within(0m), "ZMI level-1 rate");
        Assert.That(method.Bands.Single(band => band.StartExperience == 7_842).ExperiencePerHour, Is.EqualTo(28_797m).Within(0m), "ZMI level-25 rate");
        Assert.That(method.Bands.Single(band => band.StartExperience == 1_210_421).ExperiencePerHour, Is.EqualTo(57_270m).Within(0m), "ZMI level-75 rate");
        Assert.That(method.Bands.Single(band => band.StartExperience == 3_258_594).ExperiencePerHour, Is.EqualTo(72_526m).Within(0m), "ZMI level-85 rate");

        var level99 = method.Bands.Single(band => band.StartExperience == 13_034_431);
        Assert.That(level99.ExperiencePerHour, Is.EqualTo(77_121m).Within(0m), "ZMI level-99 pure-essence rate");
        Assert.That(DirectedResource(level99, 7936, TrainingFlowDirection.Input).QuantityPerExperience, Is.EqualTo(1m / level99ExperiencePerEssence).Within(0m), "ZMI pure essence per XP");
        Assert.That(DirectedResource(level99, 558, TrainingFlowDirection.Input).QuantityPerExperience, Is.EqualTo(20m / (66m * level99ExperiencePerEssence)).Within(0m), "ZMI Eniola mind runes per XP");
        Assert.That(DirectedResource(level99, 9075, TrainingFlowDirection.Input).QuantityPerExperience, Is.EqualTo(2m / (66m * level99ExperiencePerEssence)).Within(0m), "ZMI Ourania Teleport astrals per XP");
        Assert.That(DirectedResource(level99, 563, TrainingFlowDirection.Input).QuantityPerExperience, Is.EqualTo(1m / (66m * level99ExperiencePerEssence)).Within(0m), "ZMI Ourania Teleport laws per XP");
        Assert.That(DirectedResource(level99, 566, TrainingFlowDirection.Output).QuantityPerExperience, Is.EqualTo(0.09m * 1.10m * 1.7m / level99ExperiencePerEssence).Within(0m), "ZMI soul output includes diary, Raiments, and lantern");
        Assert.That(level99.Economics!.Resources.Count(resource => resource.Direction == TrainingFlowDirection.Output), Is.EqualTo(14), "ZMI prices every possible rune output");
        Assert.That(level99.Economics.Resources.All(resource => resource.ItemId is not 556 || resource.Direction == TrainingFlowDirection.Output), Is.True, "dust battlestaff removes NPC Contact air-rune inputs");

        var calculator = new TrainingPlanCalculator();
        var noDiary = calculator.Calculate(
            definition,
            13_034_431,
            13_111_552,
            new Dictionary<int, ItemPrice>(),
            methodId: method.Id,
            configuration: new Dictionary<string, string>
            {
                ["ardougne-medium-diary"] = bool.FalseString
            });
        var noDiary99 = noDiary.Method.Bands.Single(band => band.StartExperience == 13_034_431);
        Assert.That(noDiary.BaseRate, Is.EqualTo(77_121m).Within(0m), "Ardougne diary does not alter ZMI XP/hour");
        Assert.That(DirectedResource(noDiary99, 566, TrainingFlowDirection.Output).QuantityPerExperience, Is.EqualTo(0.09m * 1.7m / level99ExperiencePerEssence).Within(0m), "disabled diary removes only its soul-rune bonus");

        var daeyalt = calculator.Calculate(
            definition,
            13_034_431,
            13_150_112,
            new Dictionary<int, ItemPrice>(),
            methodId: method.Id,
            configuration: new Dictionary<string, string>
            {
                ["use-daeyalt-essence"] = bool.TrueString,
                ["daeyalt-essence-quantity"] = string.Empty
            });
        Assert.That(daeyalt.BaseRate, Is.EqualTo(115_681.5m).Within(0m), "ZMI level-99 Daeyalt rate");
        Assert.That(daeyalt.ResourceRequirements.Single(resource => resource.ItemId == 24704).Quantity, Is.EqualTo(115_681m / (level99ExperiencePerEssence * 1.5m)).Within(0.000001m), "ZMI Daeyalt essence required");
        Assert.That(daeyalt.ResourceRequirements.All(resource => resource.ItemId != 7936), Is.True, "ZMI Daeyalt replaces pure essence");

        var defaults = definition.Configurator!.Definition.Normalize();
        Assert.That(defaults.Values["ardougne-medium-diary"], Is.EqualTo(bool.TrueString), "efficient ZMI diary default");
        Assert.That(defaults.Values["abyssal-lantern-magic-logs"], Is.EqualTo(bool.TrueString), "magic-log lantern default");
    }
}
