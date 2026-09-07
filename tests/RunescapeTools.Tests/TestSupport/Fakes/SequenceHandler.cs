namespace RunescapeTools.Tests.TestSupport.Fakes;
sealed class SequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> responses = new(responses);
    public int Calls { get; private set; }
    public Uri? LastRequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastRequestUri = request.RequestUri;
        return Task.FromResult(responses.Dequeue());
    }
}
