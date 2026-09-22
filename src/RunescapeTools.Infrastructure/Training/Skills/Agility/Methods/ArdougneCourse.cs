using RunescapeTools.Core.Training;
using static RunescapeTools.Infrastructure.Training.TrainingCatalogueBuilder;

namespace RunescapeTools.Infrastructure.Training.Skills.Agility.Methods;

internal static class ArdougneCourse
{
    private const string Name = "Ardougne Rooftop Course";
    private const decimal ExperiencePerHour = 70_000m;
    // Time-gated marks: use a rounded estimate within the Wiki's 16-18.1 base range.
    private const decimal MarksPerHour = 18m;
    private const decimal AmylasePerMark = 10m;

    public static TrainingMethodDefinition Create() => new(
        "ardougne-rooftop-course", Name,
        AgilityGlobal.WithMainRouteBeforeUnlock(
            Band(AgilityGlobal.Level90Experience, ExperiencePerHour, Name,
                new TrainingEconomics([Output(Items.AmylaseCrystal, 0m,
                    quantityPerHour: MarksPerHour * AmylasePerMark)]))),
        "Standalone rooftop Agility at level 90+, without boosting. Uses a rounded efficient " +
        "70k XP/hour preset (889 XP per lap; perfect-lap ceiling approximately 70,184). " +
        "Failures below 95 and idle time can reduce actual rates; use a personal rate as needed. " +
        "Economics assume no elite Ardougne Diary and 18 marks of grace/hour, within the Wiki's " +
        "16-18.1 base range. All marks are exchanged for amylase (10 marks for 100 crystals), " +
        "sold low after GE tax. Marks are time-gated, so personal XP rates change time and the " +
        "expected total reward, not the assumed 18 marks/hour. Elite diary rewards, graceful " +
        "purchases, exchange time, travel, optional food and boosting/energy supplies are excluded. " +
        "No secondary XP or offering spells. The existing legacy route is retained below " +
        "90; its Floor 5 assumption is not a verified lower-level route.",
        UseStableDisplayName: true);

    private static class Items
    {
        public static readonly CatalogueItem AmylaseCrystal = new(12640, "Amylase crystal");
    }
}
