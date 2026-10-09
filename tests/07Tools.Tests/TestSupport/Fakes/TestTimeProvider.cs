namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
