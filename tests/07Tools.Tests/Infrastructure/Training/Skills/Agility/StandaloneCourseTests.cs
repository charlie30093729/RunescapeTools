using static RunescapeTools.Tests.TestSupport.Builders.AgilityCourseTestData;

namespace RunescapeTools.Tests.Infrastructure.Training.Skills.Agility;

[TestFixture]
[Category("Unit")]
public sealed class StandaloneCourseTests
{
    [TestCase("prifddinas-agility-course", 1210421, 54000)]
    [TestCase("prifddinas-agility-course", 1986068, 58000)]
    [TestCase("prifddinas-agility-course", 3258594, 62000)]
    [TestCase("prifddinas-agility-course", 5346332, 66000)]
    [TestCase("ardougne-rooftop-course", 5346332, 70000)]
    public void ReviewedRatesHaveOneHourAndNoSecondaryTraining(string id, long start, int rate)
    {
        var result = new TrainingPlanCalculator().Calculate(Definition(), start, start + rate,
            Prices(), methodId: id);
        Assert.That(result.BaseRate, Is.EqualTo(rate));
        Assert.That(result.Hours, Is.EqualTo(1m));
        Assert.That(result.IsFullyPriced, Is.True);
        Assert.That(result.GeneratedExperience, Is.Empty);
        Assert.That(result.ResourceRequirements.Select(item => item.ItemId),
            Does.Not.Contain(565).And.Not.Contain(21880).And.Not.Contain(31729));
    }

    [TestCase("prifddinas-agility-course", 1210421, 54000)]
    [TestCase("ardougne-rooftop-course", 5346332, 70000)]
    public void RealUnlockPreservesExistingRouteBeforeBoundary(string id, long unlock, int rate)
    {
        var definition = Definition();
        var fallback = definition.ResolveMethod(id).Bands.First();
        var original = definition.Bands.First();
        Assert.That(fallback.StartExperience, Is.EqualTo(original.StartExperience));
        Assert.That(fallback.Method, Is.EqualTo(original.Method));
        Assert.That(fallback.ExperiencePerHour, Is.EqualTo(original.ExperiencePerHour));
        Assert.That(fallback.Economics!.Resources, Is.EqualTo(original.Economics!.Resources));
        Assert.That(fallback.Economics.FixedGpOutputPerExperience,
            Is.EqualTo(original.Economics.FixedGpOutputPerExperience));
        var result = new TrainingPlanCalculator().Calculate(definition, unlock - 100, unlock + 100,
            Prices(), methodId: id);
        Assert.That(result.Bands, Has.Count.EqualTo(2));
        Assert.That(result.Bands[0].Band.Method, Is.EqualTo("Hallowed Sepulchre - Grand Coffin"));
        Assert.That(result.Bands[1].Band.ExperiencePerHour, Is.EqualTo(rate));
        Assert.That(result.Hours, Is.EqualTo(100m / 98500m + 100m / rate));
    }

    [Test]
    public void PrifSplitsHoursAcrossLevelBands()
    {
        var result = new TrainingPlanCalculator().Calculate(Definition(), 1985068, 1987068,
            Prices(), methodId: "prifddinas-agility-course");
        Assert.That(result.Bands, Has.Count.EqualTo(2));
        Assert.That(result.Hours, Is.EqualTo(1000m / 54000m + 1000m / 58000m));
    }

    [Test]
    public void PrifPricesOnlyShardConversionOnceAndCustomRatesRetainLapRewards()
    {
        var calculator = new TrainingPlanCalculator();
        var result = calculator.Calculate(Definition(), 13034431, 13034431 + 134060,
            Prices(), methodId: "prifddinas-agility-course");
        Assert.That(result.ResourceRequirements.Single(item => item.ItemId == 23962).Quantity,
            Is.EqualTo(94m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(item => item.ItemId == 12695).Quantity,
            Is.EqualTo(235m).Within(0.000001m));
        Assert.That(result.ResourceRequirements.Single(item => item.ItemId == 23685).Quantity,
            Is.EqualTo(235m).Within(0.000001m));
        Assert.That(result.NetGp, Is.EqualTo(235m * (15000m * 0.98m - 12000m)).Within(0.01m));
        var faster = calculator.Calculate(Definition(), 13034431, 13034431 + 134060,
            Prices(), 132000m, "prifddinas-agility-course");
        Assert.That(faster.Hours, Is.EqualTo(result.Hours / 2m));
        Assert.That(faster.ResourceRequirements, Is.EqualTo(result.ResourceRequirements));
        Assert.That(faster.NetGp, Is.EqualTo(result.NetGp));
    }

    [TestCase(70000, 180)]
    [TestCase(35000, 360)]
    public void ArdougneMarksAreTimeBasedAndAmylaseSellsLowAfterTax(int rate, int amylase)
    {
        var result = new TrainingPlanCalculator().Calculate(Definition(), 13034431, 13104431,
            Prices(), rate, "ardougne-rooftop-course");
        Assert.That(result.ResourceRequirements, Has.Count.EqualTo(1));
        Assert.That(result.ResourceRequirements.Single().ItemId, Is.EqualTo(12640));
        Assert.That(result.ResourceRequirements.Single().Quantity, Is.EqualTo(amylase));
        Assert.That(result.NetGp, Is.EqualTo(amylase * 980m).Within(0.01m));
    }

    [TestCase("prifddinas-agility-course", 12695)]
    [TestCase("prifddinas-agility-course", 23685)]
    [TestCase("ardougne-rooftop-course", 12640)]
    public void MissingCourseQuotesStayVisiblyUnpriced(string id, int missingId)
    {
        var prices = Prices();
        prices.Remove(missingId);
        var result = new TrainingPlanCalculator().Calculate(Definition(), 13034431, 14000000,
            prices, methodId: id);
        Assert.That(result.HasMissingPrice, Is.True);
        Assert.That(result.IsFullyPriced, Is.False);
        Assert.That(result.NetGp, Is.Null);
        Assert.That(Definition().MarketItemIds, Does.Contain(missingId));
    }
}
