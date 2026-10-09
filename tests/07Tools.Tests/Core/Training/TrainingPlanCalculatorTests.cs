namespace RunescapeTools.Tests.Core.Training;

[TestFixture]
[Category("Unit")]
public sealed class TrainingPlanCalculatorTests
{
    [Test]
    [Property("LegacyScenario", "TrainingResourceRequirements")]
    [Description("training plans aggregate full-route item quantities")]
    public void TrainingResourceRequirements()
    {
        var definition = new TrainingSkillDefinition(
            "Test",
            [
                new TrainingRateBand(
                    0,
                    100m,
                    "First band",
                    new TrainingEconomics(
                    [
                        new TrainingResourceFlow(1, "Input", 0.5m, TrainingFlowDirection.Input, QuantityPerHour: 2m),
                        new TrainingResourceFlow(2, "Output", 0.1m, TrainingFlowDirection.Output)
                    ])),
                new TrainingRateBand(
                    100,
                    200m,
                    "Second band",
                    new TrainingEconomics(
                    [
                        new TrainingResourceFlow(1, "Input", 0.25m, TrainingFlowDirection.Input, QuantityPerHour: 2m),
                        new TrainingResourceFlow(2, "Output", 0.2m, TrainingFlowDirection.Output)
                    ]))
            ]);
        var prices = new Dictionary<int, ItemPrice>
        {
            [1] = Quote(1, 100),
            [2] = Quote(2, 100)
        };

        var result = new TrainingPlanCalculator().Calculate(definition, 0, 200, prices);

        Assert.That(result.ResourceRequirements.Count, Is.EqualTo(2), "aggregated resource count");
        Assert.That(result.ResourceRequirements.Single(item => item.ItemId == 1).Quantity, Is.EqualTo(78m).Within(0m), "per-XP and per-hour inputs across both bands");
        Assert.That(result.ResourceRequirements.Single(item => item.ItemId == 2).Quantity, Is.EqualTo(30m).Within(0m), "expected outputs across both bands");
    }

    [Test]
    [Property("LegacyScenario", "TrainingRateOverride")]
    [Description("training rate overrides scale hours without changing total resources")]
    public void TrainingRateOverride()
    {
        var definition = new MainEhpCatalogue().Skills.Single(skill => skill.Skill == "Construction");
        var prices = new Dictionary<int, ItemPrice> { [8778] = Quote(8778, 431), [8782] = Quote(8782, 1_910) };
        var calculator = new TrainingPlanCalculator();
        var baseline = calculator.Calculate(definition, 0, 200_000_000, prices);
        var doubled = calculator.Calculate(definition, 0, 200_000_000, prices, 109_400m);

        Assert.That(doubled.Hours, Is.EqualTo(baseline.Hours / 2m).Within(0.0001m), "double-rate hours");
        Assert.That(doubled.NetGp ?? 0m, Is.EqualTo(baseline.NetGp ?? 0m).Within(0.01m), "rate override total GP");
    }

    [Test]
    [Property("LegacyScenario", "ConfiguredTrainingRateOverrides")]
    [Description("configured XP gains apply on top of personal base rates")]
    public void ConfiguredTrainingRateOverrides()
    {
        var catalogue = new MainEhpCatalogue();
        var calculator = new TrainingPlanCalculator();

        var runecraft = catalogue.Skills.Single(skill => skill.Skill == "Runecraft");
        const long runecraftStart = 13_034_431;
        const long daeyaltExperience = 15_750;
        var runecraftResult = calculator.Calculate(
            runecraft,
            runecraftStart,
            runecraftStart + daeyaltExperience * 2,
            new Dictionary<int, ItemPrice>
            {
                [7936] = Quote(7936, 1),
                [5521] = Quote(5521, 1_000),
                [9075] = Quote(9075, 100),
                [557] = Quote(557, 5),
                [4699] = Quote(4699, 20)
            },
            personalRate: 100_000m,
            methodId: "solo-lava-runes",
            configuration: new Dictionary<string, string>
            {
                ["use-daeyalt-essence"] = bool.TrueString,
                ["daeyalt-essence-quantity"] = "1000"
            });
        Assert.That(runecraftResult.EffectiveRate, Is.EqualTo(150_000m).Within(0m), "Daeyalt personal effective rate");
        Assert.That(runecraftResult.Hours, Is.EqualTo(daeyaltExperience / 150_000m + daeyaltExperience / 100_000m).Within(0m), "Daeyalt custom rate applies only while its stock lasts");

        var firemaking = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Firemaking"),
            13_034_431,
            13_034_431 + 246_000,
            new Dictionary<int, ItemPrice>(),
            personalRate: 240_000m,
            methodId: "redwood-logs",
            configuration: new Dictionary<string, string>
            {
                ["pyromancer-outfit"] = bool.TrueString,
                ["bonfire"] = bool.FalseString
            });
        Assert.That(firemaking.EffectiveRate, Is.EqualTo(246_000m).Within(0m), "Pyromancer personal effective rate");
        Assert.That(firemaking.Hours, Is.EqualTo(1m).Within(0.000001m), "Pyromancer custom-rate hours");

        var construction = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Construction"),
            13_034_431,
            14_059_431,
            new Dictionary<int, ItemPrice>(),
            personalRate: 1_000_000m,
            configuration: new Dictionary<string, string>
            {
                ["carpenters-outfit"] = bool.TrueString
            });
        Assert.That(construction.EffectiveRate, Is.EqualTo(1_025_000m).Within(0m), "Carpenter personal effective rate");
        Assert.That(construction.Hours, Is.EqualTo(1m).Within(0.000001m), "Carpenter custom-rate hours");

        var smithing = calculator.Calculate(
            catalogue.Skills.Single(skill => skill.Skill == "Smithing"),
            4_382_299,
            4_707_299,
            new Dictionary<int, ItemPrice>(),
            personalRate: 260_400m,
            methodId: "adamant-platebodies",
            configuration: new Dictionary<string, string>
            {
                ["smiths-uniform"] = bool.TrueString
            });
        Assert.That(smithing.EffectiveRate, Is.EqualTo(325_000m).Within(0.000001m), "Smiths' uniform personal effective rate");
        Assert.That(smithing.Hours, Is.EqualTo(1m).Within(0.000001m), "Smiths' uniform custom-rate hours");
    }

    [Test]
    [Property("LegacyScenario", "HourlyTrainingEconomics")]
    [Description("hourly training costs respond to personal rate overrides")]
    public void HourlyTrainingEconomics()
    {
        var definition = new TrainingSkillDefinition(
            "Hourly test",
            [
                new TrainingRateBand(
                    0,
                    100m,
                    "Hourly method",
                    new TrainingEconomics(
                        [
                            new TrainingResourceFlow(
                                1,
                                "Hourly supply",
                                0m,
                                TrainingFlowDirection.Input,
                                QuantityPerHour: 10m)
                        ],
                        FixedGpPerHour: 100m))
            ]);
        var prices = new Dictionary<int, ItemPrice> { [1] = Quote(1, 100) };
        var calculator = new TrainingPlanCalculator();

        var baseline = calculator.Calculate(definition, 0, 100, prices);
        var doubled = calculator.Calculate(definition, 0, 100, prices, 200m);

        Assert.That(baseline.Hours, Is.EqualTo(1m).Within(0m), "baseline hourly-method hours");
        Assert.That(baseline.NetGp ?? 0m, Is.EqualTo(-1_100m).Within(0m), "baseline hourly-method cost");
        Assert.That(doubled.Hours, Is.EqualTo(0.5m).Within(0m), "doubled hourly-method hours");
        Assert.That(doubled.NetGp ?? 0m, Is.EqualTo(-550m).Within(0m), "doubled hourly-method cost");
    }
}
