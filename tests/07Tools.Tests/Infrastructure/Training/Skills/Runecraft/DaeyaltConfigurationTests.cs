namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Runecraft;

[TestFixture]
[Category("Unit")]
public sealed class DaeyaltConfigurationTests
{
    [Test]
    [Property("LegacyScenario", "RunecraftDaeyaltConfiguration")]
    [Description("Runecraft Daeyalt configuration supports unlimited and finite stock")]
    public void RunecraftDaeyaltConfiguration()
    {
        const long startExperience = 13_034_431;
        const long daeyaltExperience = 15_750;
        var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Runecraft");
        var calculator = new TrainingPlanCalculator();
        var prices = new Dictionary<int, ItemPrice>
        {
            [7936] = Quote(7936, 1),
            [5521] = Quote(5521, 1_000),
            [9075] = Quote(9075, 100),
            [557] = Quote(557, 5),
            [4699] = Quote(4699, 20)
        };
        var unlimitedConfiguration = new Dictionary<string, string>
        {
            ["use-daeyalt-essence"] = bool.TrueString,
            ["daeyalt-essence-quantity"] = string.Empty
        };

        var unlimited = calculator.Calculate(
            definition,
            startExperience,
            startExperience + daeyaltExperience,
            prices,
            methodId: "solo-lava-runes",
            configuration: unlimitedConfiguration);
        Assert.That(unlimited.BaseRate, Is.EqualTo(153_150m).Within(0m), "Daeyalt lava XP/hour");
        Assert.That(unlimited.Hours, Is.EqualTo(daeyaltExperience / 153_150m).Within(0m), "Daeyalt lava hours");
        Assert.That(unlimited.ResourceRequirements.Single(resource => resource.ItemId == 24704).Quantity, Is.EqualTo(1_000m).Within(0.000001m), "unlimited Daeyalt quantity");
        Assert.That(!unlimited.ResourceRequirements.Single(resource => resource.ItemId == 24704).RequiresMarketPrice, Is.True, "Daeyalt is supplied untradeable stock");
        Assert.That(unlimited.ResourceRequirements.All(resource => resource.ItemId != 7936), Is.True, "unlimited Daeyalt replaces pure essence");
        Assert.That(unlimited.IsFullyPriced, Is.True, "Daeyalt does not require a GE quote");

        var finite = calculator.Calculate(
            definition,
            startExperience,
            startExperience + daeyaltExperience * 2,
            prices,
            methodId: "solo-lava-runes",
            configuration: new Dictionary<string, string>
            {
                ["use-daeyalt-essence"] = bool.TrueString,
                ["daeyalt-essence-quantity"] = "1,000"
            });
        Assert.That(finite.Bands.Count, Is.EqualTo(2), "finite Daeyalt splits the calculated route");
        Assert.That(finite.Bands[0].Experience, Is.EqualTo(daeyaltExperience), "Daeyalt segment XP");
        Assert.That(finite.Bands[0].Band.Method.EndsWith("Daeyalt essence", StringComparison.Ordinal), Is.True, "Daeyalt segment label");
        Assert.That(!finite.Bands[1].Band.Method.EndsWith("Daeyalt essence", StringComparison.Ordinal), Is.True, "pure essence fallback label");
        Assert.That(finite.Hours, Is.EqualTo(daeyaltExperience / 153_150m + daeyaltExperience / 102_100m).Within(0m), "finite Daeyalt then pure essence hours");
        Assert.That(finite.ResourceRequirements.Single(resource => resource.ItemId == 24704).Quantity, Is.EqualTo(1_000m).Within(0.000001m), "finite Daeyalt stock consumed");
        Assert.That(finite.ResourceRequirements.Single(resource => resource.ItemId == 7936).Quantity, Is.EqualTo(1_500m).Within(0.000001m), "pure essence resumes after Daeyalt stock");

        var disabled = calculator.Calculate(
            definition,
            startExperience,
            startExperience + daeyaltExperience,
            prices,
            methodId: "solo-lava-runes",
            configuration: new Dictionary<string, string>
            {
                ["use-daeyalt-essence"] = bool.FalseString,
                ["daeyalt-essence-quantity"] = "1,000"
            });
        Assert.That(disabled.BaseRate, Is.EqualTo(102_100m).Within(0m), "disabled Daeyalt uses the normal rate");
        Assert.That(disabled.ResourceRequirements.All(resource => resource.ItemId != 24704), Is.True, "disabled Daeyalt is ignored");

        var arceuus = calculator.Calculate(
            definition,
            1_475_581,
            1_511_581,
            new Dictionary<int, ItemPrice> { [565] = Quote(565, 1_000) },
            methodId: "arceuus-blood-runes",
            configuration: unlimitedConfiguration);
        Assert.That(arceuus.BaseRate, Is.EqualTo(36_000m).Within(0m), "Daeyalt does not alter dark-essence blood runes");
        Assert.That(arceuus.ResourceRequirements.All(resource => resource.ItemId != 24704), Is.True, "Arceuus route uses no Daeyalt");

        var configurationDefinition = definition.Configurator!.Definition;
        var defaults = configurationDefinition.Normalize();
        Assert.That(defaults.Values["use-daeyalt-essence"], Is.EqualTo(bool.FalseString), "Daeyalt toggle default");
        Assert.That(defaults.Values["daeyalt-essence-quantity"], Is.EqualTo(string.Empty), "blank Daeyalt quantity means unlimited");
        var dialog = new TrainingConfigurationDialogViewModel(
            "Runecraft",
            "Solo lava runes",
            configurationDefinition,
            defaults.Values,
            "solo-lava-runes");
        var quantity = dialog.Options.Single(option => option.Key == "daeyalt-essence-quantity");
        Assert.That(quantity.IsNumber && quantity.IsValid, Is.True, "Daeyalt quantity is an optional numeric field");
        quantity.NumberValue = "1,000";
        Assert.That(quantity.IsValid, Is.True, "formatted whole-number Daeyalt quantity is valid");
        Assert.That(dialog.ToValues()["daeyalt-essence-quantity"], Is.EqualTo("1000"), "Daeyalt quantity normalizes for persistence");
        quantity.NumberValue = "1.5";
        Assert.That(!quantity.IsValid && !dialog.IsValid, Is.True, "fractional Daeyalt quantities are rejected");

        var priceDialog = new TrainingPriceDialogViewModel("Runecraft", unlimited, prices);
        var daeyaltRow = priceDialog.Items.Single(item => item.ItemId == 24704);
        Assert.That(daeyaltRow.Action, Is.EqualTo("USE"), "Daeyalt stock action");
        Assert.That(daeyaltRow.UnitPrice, Is.EqualTo("Untradeable"), "Daeyalt stock has no GE price");

        var plannerRow = new XpPlannerRowViewModel(
            definition,
            calculator,
            startExperience,
            null,
            prices,
            () => { });
        plannerRow.ApplyConfiguration(new Dictionary<string, string>
        {
            ["use-daeyalt-essence"] = bool.TrueString,
            ["daeyalt-essence-quantity"] = "1,000"
        });
        var savedConfiguration = plannerRow.ToPreference().Configuration!;
        Assert.That(savedConfiguration["use-daeyalt-essence"], Is.EqualTo(bool.TrueString), "Daeyalt toggle persists per profile");
        Assert.That(savedConfiguration["daeyalt-essence-quantity"], Is.EqualTo("1000"), "Daeyalt quantity persists per profile");
        plannerRow.PersonalRate = 100_000m;
        Assert.That(plannerRow.PersonalRate, Is.EqualTo(150_000m).Within(0m), "planner displays the Daeyalt-adjusted personal rate");
        Assert.That(plannerRow.Result.EffectiveRate, Is.EqualTo(150_000m).Within(0m), "planner applies Daeyalt to the typed rate");
        Assert.That(plannerRow.ToPreference().ExperiencePerHourOverride ?? 0m, Is.EqualTo(100_000m).Within(0m), "planner persists the personal base rate");
        plannerRow.ApplyConfiguration(new Dictionary<string, string>
        {
            ["use-daeyalt-essence"] = bool.FalseString,
            ["daeyalt-essence-quantity"] = "1,000"
        });
        Assert.That(plannerRow.PersonalRate, Is.EqualTo(100_000m).Within(0m), "disabling Daeyalt displays the personal base rate");
        Assert.That(plannerRow.Result.EffectiveRate, Is.EqualTo(100_000m).Within(0m), "disabling Daeyalt preserves the typed base rate");

        var natureRow = new XpPlannerRowViewModel(
            definition,
            calculator,
            startExperience,
            null,
            prices,
            () => { });
        natureRow.SelectedMethodOption = natureRow.MethodOptions.Single(option =>
            option.Id == "achievement-cape-double-nature-runes");
        natureRow.PersonalRate = 75_000m;
        Assert.That(natureRow.PersonalRate, Is.EqualTo(75_000m).Within(0m), "double-nature custom rate before Daeyalt");
        natureRow.ApplyConfiguration(new Dictionary<string, string>
        {
            ["use-daeyalt-essence"] = bool.TrueString,
            ["daeyalt-essence-quantity"] = string.Empty
        });
        Assert.That(natureRow.PersonalRate, Is.EqualTo(112_500m).Within(0m), "double-nature XP/hour displays the Daeyalt bonus");
        Assert.That(natureRow.Result.EffectiveRate, Is.EqualTo(112_500m).Within(0m), "double-nature calculation uses the Daeyalt bonus");
        Assert.That(natureRow.ToPreference().ExperiencePerHourOverride ?? 0m, Is.EqualTo(75_000m).Within(0m), "double-nature plan persists the pre-Daeyalt personal base rate");

        var doloAetherRow = new XpPlannerRowViewModel(
            definition,
            calculator,
            5_346_332,
            null,
            prices,
            () => { });
        doloAetherRow.SelectedMethodOption = doloAetherRow.MethodOptions.Single(option =>
            option.Id == "dolo-aether-runes");
        doloAetherRow.PersonalRate = 75_000m;
        doloAetherRow.ApplyConfiguration(new Dictionary<string, string>
        {
            ["use-daeyalt-essence"] = bool.TrueString,
            ["daeyalt-essence-quantity"] = string.Empty
        });
        var expectedDoloDaeyaltRate = 75_000m * (1m + (63m / 109m * 0.5m));
        Assert.That(doloAetherRow.PersonalRate, Is.EqualTo(expectedDoloDaeyaltRate).Within(0m), "dolo Aether custom rate boosts only the main-account share");
        Assert.That(doloAetherRow.Result.EffectiveRate, Is.EqualTo(expectedDoloDaeyaltRate).Within(0m), "dolo Aether calculation applies its partial Daeyalt multiplier");
        Assert.That(doloAetherRow.ToPreference().ExperiencePerHourOverride ?? 0m, Is.EqualTo(75_000m).Within(0m), "dolo Aether plan persists the pre-Daeyalt personal base rate");
    }
}
