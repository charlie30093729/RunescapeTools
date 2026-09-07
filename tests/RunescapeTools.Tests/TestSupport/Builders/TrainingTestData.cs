namespace RunescapeTools.Tests.TestSupport.Builders;
internal static class TrainingTestData
{
    public static TrainingRateBand TrainingBand(MainEhpCatalogue catalogue, string skill, long startExperience) =>
        catalogue.Skills.Single(definition => definition.Skill == skill)
            .Bands.Single(band => band.StartExperience == startExperience);

    public static TrainingResourceFlow Resource(TrainingRateBand band, int itemId) =>
        band.Economics?.Resources.Single(resource => resource.ItemId == itemId)
        ?? throw new InvalidOperationException($"{band.Method} is missing item {itemId}.");

    public static TrainingResourceFlow DirectedResource(
        TrainingRateBand band,
        int itemId,
        TrainingFlowDirection direction) =>
        band.Economics?.Resources.Single(resource =>
            resource.ItemId == itemId && resource.Direction == direction)
        ?? throw new InvalidOperationException($"{band.Method} is missing {direction} item {itemId}.");

    public static decimal TotalResourceQuantity(
        MainEhpCatalogue catalogue,
        string skill,
        int itemId,
        long startExperience = 0,
        long targetExperience = TrainingPlanCalculator.MaximumExperience)
    {
        var definition = catalogue.Skills.Single(value => value.Skill == skill);
        var ordered = definition.Bands.OrderBy(band => band.StartExperience).ToArray();
        decimal total = 0m;
        for (var index = 0; index < ordered.Length; index++)
        {
            var band = ordered[index];
            var nextStart = index + 1 < ordered.Length
                ? ordered[index + 1].StartExperience
                : TrainingPlanCalculator.MaximumExperience;
            var segmentStart = Math.Max(startExperience, band.StartExperience);
            var segmentEnd = Math.Min(targetExperience, nextStart);
            if (segmentEnd <= segmentStart)
                continue;

            var resource = band.Economics?.Resources.SingleOrDefault(value => value.ItemId == itemId);
            if (resource is null)
                continue;
            var experience = segmentEnd - segmentStart;
            var hours = experience / band.ExperiencePerHour;
            total += resource.QuantityPerExperience * experience + resource.QuantityPerHour * hours;
        }

        return total;
    }
}
