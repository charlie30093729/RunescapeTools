namespace RunescapeTools.Tests.Wpf.ViewModels;

[TestFixture]
[Category("Unit")]
public sealed class ProfileViewModelTests
{
    [Test]
    [Property("LegacyScenario", "ProfileViewModelFlow")]
    [Description("profile view-model loads defaults and keeps valid data on errors")]
    public async Task ProfileViewModelFlow()
    {
        var client = new FakeHiscoreClient();
        var preference = new MemoryProfilePreferenceStore("bottleo");
        var context = new CurrentProfileContext(client, new HiscoreParser(TimeProvider.System), preference);
        var viewModel = new ProfileViewModel(context);

        await viewModel.LoadAsync();
        Assert.That(viewModel.ProfileRsn, Is.EqualTo("bottleo"), "default profile RSN");
        Assert.That(viewModel.Skills.Count, Is.EqualTo(24), "displayed skill count");

        client.Handler = (rsn, _) => throw new PlayerNotFoundException(rsn);
        viewModel.SearchRsn = "does not exist";
        await viewModel.SearchCommand.ExecuteAsync(null);
        Assert.That(viewModel.ProfileRsn, Is.EqualTo("bottleo"), "failed search retains profile");
        Assert.That(!string.IsNullOrWhiteSpace(viewModel.ErrorMessage), Is.True, "failed search error");

        client.Handler = (_, _) => Task.FromResult(HiscoreResponse(75));
        viewModel.SearchRsn = "  New Player  ";
        await viewModel.SearchCommand.ExecuteAsync(null);
        Assert.That(viewModel.ProfileRsn, Is.EqualTo("New Player"), "successful searched profile");
        Assert.That(preference.SelectedRsn, Is.EqualTo("New Player"), "successful search persisted");
        Assert.That(viewModel.Skills[0].Level, Is.EqualTo("75"), "updated skill level");
    }
}
