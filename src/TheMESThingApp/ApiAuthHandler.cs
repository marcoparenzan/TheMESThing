namespace TheMESThingApp;

public class ApiAuthHandler : DelegatingHandler
{
    private readonly string _apiKey;

    public ApiAuthHandler(IConfiguration config)
    {
        _apiKey = config["ApiKey"] ?? throw new InvalidOperationException("ApiKey not configured.");
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Api-Key", _apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
