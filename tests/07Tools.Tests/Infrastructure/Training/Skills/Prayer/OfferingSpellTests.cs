using static RunescapeTools.Tests.TestSupport.Builders.PrayerOfferingTestData;

namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Prayer;

[TestFixture]
[Category("Unit")]
public sealed class OfferingSpellTests
{
    [TestCase("dragon-bones", 536, 388_800, 0)]
    [TestCase("frost-dragon-bones", 31729, 540_000, 0)]
    [TestCase("superior-dragon-bones", 22124, 810_000, 737_627)]
    public void BankOfferingUsesSixHundredFullCastsPerHour(string method, int boneId, int rate, long start)
    {
        var result = new TrainingPlanCalculator().Calculate(
            CreatePrayerDefinition(), start, start + rate, Prices(), methodId: method,
            configuration: Location("offering-at-bank"));

        Assert.That(result.BaseRate, Is.EqualTo(rate));
        Assert.That(result.Hours, Is.EqualTo(1m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == boneId).Quantity,
            Is.EqualTo(1_800m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 565).Quantity,
            Is.EqualTo(600m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 21880).Quantity,
            Is.EqualTo(600m).Within(0.000001m));
        Assert.That(result.GeneratedExperience["Magic"], Is.EqualTo(108_000m).Within(0.000001m));
        Assert.That(result.GeneratedExperience.ContainsKey("Agility"), Is.False);
        Assert.That(result.ResourceRequirements, Has.Count.EqualTo(3));
        Assert.That(result.HasMissingPrice, Is.False);
    }

    [TestCase("dragon-bones", 536, 72, 0)]
    [TestCase("frost-dragon-bones", 31729, 100, 0)]
    [TestCase("superior-dragon-bones", 22124, 150, 737_627)]
    public void PrifLapUsesEightCastsAndCreditsOneLap(string method, int boneId, int burialXp, long start)
    {
        var prayerXpPerLap = burialXp * 3 * 24;
        var result = new TrainingPlanCalculator().Calculate(
            CreatePrayerDefinition(), start, start + prayerXpPerLap, Prices(), methodId: method,
            configuration: Location("offering-at-prif-agility"));

        Assert.That(result.BaseRate, Is.EqualTo(prayerXpPerLap * 3_600m / 82m));
        Assert.That(result.Hours, Is.EqualTo(82m / 3_600m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == boneId).Quantity,
            Is.EqualTo(24m).Within(0.000001m));
        Assert.That(result.GeneratedExperience["Magic"], Is.EqualTo(1_440m).Within(0.000001m));
        Assert.That(result.GeneratedExperience["Agility"], Is.EqualTo(1_340.6m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 23962).Quantity,
            Is.EqualTo(0.94m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 12695).Quantity,
            Is.EqualTo(2.35m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 23685).Quantity,
            Is.EqualTo(2.35m).Within(0.000001m));
        Assert.That(result.HasMissingPrice, Is.False, "Untradeable shards do not need a GE quote");
    }

    [Test]
    public void FrostBonesFullPrifRouteMatchesReviewedHoursAndPricesEachFlowOnce()
    {
        var result = new TrainingPlanCalculator().Calculate(
            CreatePrayerDefinition(), 0, 200_000_000, Prices(), methodId: "frost-dragon-bones",
            configuration: Location("offering-at-prif-agility"));

        Assert.That(result.Hours, Is.EqualTo(632.716049m).Within(0.000001m));
        Assert.That(result.GeneratedExperience["Magic"], Is.EqualTo(40_000_000m).Within(0.000001m));
        Assert.That(result.GeneratedExperience["Agility"], Is.EqualTo(37_238_888.888889m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 31729).Quantity,
            Is.EqualTo(666_666.666667m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 23962).Quantity,
            Is.EqualTo(26_111.111111m).Within(0.000001m));
        // 666,666.667 bones * 6,000 + 222,222.222 casts * 900 input GP;
        // 65,277.778 conversions * (15,000 low - 300 tax - 12,000 high).
        Assert.That(result.NetGp, Is.EqualTo(-4_023_750_000m).Within(0.01m));
        Assert.That(result.GpPerExperience, Is.EqualTo(-20.11875m).Within(0.000001m));
        Assert.That(result.IncludesActiveHours, Is.True);
    }

    [TestCase("offering-at-bank")]
    [TestCase("offering-at-prif-agility")]
    public void PersonalRateChangesHoursButNotGoalResourcesOrSecondaryXp(string location)
    {
        var calculator = new TrainingPlanCalculator();
        var baseline = calculator.Calculate(CreatePrayerDefinition(), 0, 900_000, Prices(),
            methodId: "frost-dragon-bones", configuration: Location(location));
        var faster = calculator.Calculate(CreatePrayerDefinition(), 0, 900_000, Prices(),
            personalRate: baseline.BaseRate * 2m, methodId: "frost-dragon-bones", configuration: Location(location));

        Assert.That(faster.Hours, Is.EqualTo(baseline.Hours / 2m).Within(0.000001m));
        Assert.That(faster.ResourceRequirements, Is.EqualTo(baseline.ResourceRequirements));
        Assert.That(faster.GeneratedExperience, Is.EqualTo(baseline.GeneratedExperience));
        Assert.That(faster.NetGp, Is.EqualTo(baseline.NetGp));
    }

    [TestCase("offering-at-bank")]
    [TestCase("offering-at-prif-agility")]
    public void SuperiorBonesUseDragonBonesUntilLevelSeventy(string location)
    {
        var result = new TrainingPlanCalculator().Calculate(CreatePrayerDefinition(), 737_626, 737_628, Prices(),
            methodId: "superior-dragon-bones", configuration: Location(location));

        Assert.That(result.Bands.Select(band => band.Experience), Is.EqualTo(new long[] { 1, 1 }));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 536).Quantity,
            Is.EqualTo(1m / 216m));
        Assert.That(result.ResourceRequirements.Single(r => r.ItemId == 22124).Quantity,
            Is.EqualTo(1m / 450m));
    }

    [Test]
    public void MissingWrathPriceIsNotTreatedAsFreeCasting()
    {
        var prices = Prices();
        prices.Remove(21880);
        var result = new TrainingPlanCalculator().Calculate(CreatePrayerDefinition(), 0, 900_000, prices,
            methodId: "frost-dragon-bones", configuration: Location("offering-at-bank"));
        Assert.That(result.HasMissingPrice, Is.True);
        Assert.That(result.NetGp, Is.Null);
    }

    [Test]
    public void PriceDiscoveryIncludesNonDefaultConfigurationResources()
    {
        Assert.That(CreatePrayerDefinition().MarketItemIds, Is.SupersetOf(new[] { 565, 21880, 12695, 23685 }));
        Assert.That(CreatePrayerDefinition().MarketItemIds, Does.Not.Contain(23962));
    }

    [Test]
    public void SwitchingBackToAnAltarRemovesSpellAndCourseFlows()
    {
        var result = new TrainingPlanCalculator().Calculate(CreatePrayerDefinition(), 0, 700_000, Prices(),
            methodId: "frost-dragon-bones", configuration: Location("chaos-altar"));
        Assert.That(result.GeneratedExperience, Is.Empty);
        Assert.That(result.ResourceRequirements.Select(r => r.ItemId), Is.EqualTo(new[] { 31729 }));
    }
}
