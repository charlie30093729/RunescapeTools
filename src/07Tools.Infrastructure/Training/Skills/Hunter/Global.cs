using RunescapeTools.Core.Training;

namespace RunescapeTools.Infrastructure.Training.Skills.Hunter;

internal static class HunterGlobal
{
    public const string ThreeTickKey = "three-tick";
    public const string ShootingAltKey = "shooting-alt";
    private const long BlackUnlock = 992_895;
    private const long RedUnlock = 368_599;
    private const decimal BlackAltRate = 265_000m;
    private const decimal BlackSoloThreeTickRate = 225_000m;
    // User-approved efficient solo planning estimate, not a measured Wiki benchmark.
    private const decimal BlackNormalRate = 200_000m;

    public static ITrainingSkillConfigurator Configurator { get; } = new TrainingSkillConfigurator(
        new TrainingConfigurationDefinition(
        [
            new(ThreeTickKey, "3-tick hunting", TrainingConfigurationOptionKind.Toggle,
                bool.TrueString, "Use tick-manipulation rates for chinchompas. Personal rate overrides " +
                "scale by the configured/default rate ratio; catch XP and total chins do not change.",
                ApplicableMethodIds: ["main-ehp", "red-chinchompas"]),
            new(ShootingAltKey, "Shooting alt", TrainingConfigurationOptionKind.Toggle,
                bool.TrueString, "Black chinchompas only. Requires 3-tick hunting; disabling 3-ticking " +
                "also switches this option off. Alt ammunition costs are excluded.",
                ApplicableMethodIds: ["main-ehp"], RequiredToggleKey: ThreeTickKey)
        ]), ConfigureMethod);

    private static TrainingMethodDefinition ConfigureMethod(
        TrainingMethodDefinition method, TrainingConfigurationValues values)
    {
        if (method.Id is not "main-ehp" and not "red-chinchompas")
            return method;

        var threeTick = values.GetToggle(ThreeTickKey);
        var shootingAlt = threeTick && values.GetToggle(ShootingAltKey);
        return method with
        {
            Bands = method.Bands.Select(band =>
            {
                decimal rate;
                if (method.Id == "main-ehp" && band.StartExperience >= BlackUnlock)
                    rate = !threeTick ? BlackNormalRate : shootingAlt ? BlackAltRate : BlackSoloThreeTickRate;
                else if (method.Id == "red-chinchompas" && band.StartExperience >= RedUnlock && !threeTick)
                    rate = band.StartExperience switch
                    {
                        >= 13_034_431 => 170_000m,
                        >= 5_346_332 => 136_000m,
                        >= 1_986_068 => 115_000m,
                        >= 737_627 => 72_000m,
                        _ => 61_000m
                    };
                else
                    return band;

                // Throughput, not an XP-per-catch bonus: never scale material/XP flows.
                return band with
                {
                    ExperiencePerHour = rate,
                    ConfigurationRateMultiplier = rate / band.ExperiencePerHour
                };
            }).ToArray()
        };
    }
}
