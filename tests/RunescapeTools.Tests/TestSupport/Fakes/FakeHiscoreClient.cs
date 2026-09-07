namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class FakeHiscoreClient : IHiscoreClient
{
    public Func<string, CancellationToken, Task<string>> Handler { get; set; }
        = (_, _) => Task.FromResult(CreateResponse());

    public Task<string> GetRawHiscoresAsync(string rsn, CancellationToken cancellationToken = default) =>
        Handler(rsn, cancellationToken);

    private static string CreateResponse()
    {
        var rows = new List<string> { "123,2376,4567890123" };
        rows.AddRange(Enumerable.Range(0, OsrsHiscoreSkillOrder.Skills.Count)
            .Select(index => $"{1_000 + index},99,{13_034_431L + index}"));
        return string.Join('\n', rows);
    }
}
