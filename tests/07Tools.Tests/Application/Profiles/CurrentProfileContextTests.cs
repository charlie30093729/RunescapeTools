namespace RunescapeTools.Tests.Application.Profiles;

[TestFixture]
[Category("Unit")]
public sealed class CurrentProfileContextTests
{
    [Test]
    [Property("LegacyScenario", "ProfileContextStateFlow")]
    [Description("profile context preserves valid state on failure and publishes refreshes")]
    public async Task ProfileContextStateFlow()
    {
        var client = new FakeHiscoreClient();
        var preference = new MemoryProfilePreferenceStore("bottleo");
        var context = new CurrentProfileContext(
            client,
            new HiscoreParser(new TestTimeProvider(new DateTimeOffset(2026, 7, 18, 0, 0, 0, TimeSpan.Zero))),
            preference);
        var changes = 0;
        context.ProfileChanged += (_, _) => changes++;

        await context.LoadSelectedProfileAsync();
        Assert.That(context.CurrentRsn ?? string.Empty, Is.EqualTo("bottleo"), "loaded saved profile");
        Assert.That(changes, Is.EqualTo(1), "initial notification");

        client.Handler = (rsn, _) => throw new PlayerNotFoundException(rsn);
        await Assert.ThatAsync((Func<Task>)(() => context.LoadProfileAsync("missing")), Throws.InstanceOf<PlayerNotFoundException>(), "failed selection");
        Assert.That(context.CurrentRsn ?? string.Empty, Is.EqualTo("bottleo"), "valid profile retained");
        Assert.That(preference.SelectedRsn, Is.EqualTo("bottleo"), "failed RSN not persisted");
        Assert.That(changes, Is.EqualTo(1), "failed load does not notify");

        client.Handler = async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return HiscoreResponse();
        };
        using (var cancellation = new CancellationTokenSource())
        {
            cancellation.Cancel();
            await Assert.ThatAsync((Func<Task>)(() => context.LoadProfileAsync("cancelled", cancellation.Token)), Throws.InstanceOf<OperationCanceledException>(), "cancelled profile request");
        }
        Assert.That(context.CurrentRsn ?? string.Empty, Is.EqualTo("bottleo"), "cancellation retains profile");
        Assert.That(changes, Is.EqualTo(1), "cancellation does not notify");

        client.Handler = (_, _) => Task.FromResult(HiscoreResponse(98));
        await context.RefreshAsync();
        Assert.That(changes, Is.EqualTo(2), "refresh notification");
        Assert.That(context.CurrentProfile?.Skills[0].Level ?? 0, Is.EqualTo(98), "refreshed profile data");
    }
}
