using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using Remora.Results;
using VTP.AntiPhishingGateway.Errors;

namespace VTP.AntiPhishingGateway;

public class PhishingGatewayService
{
    private readonly HashSet<string> _domains = new();
    private readonly HttpClient _http;

    private const string HeaderName   = "X-Identity";
    private const string ApiUrl       = "https://phish.sinking.yachts/v2/all";
    private const string WebSocketUrl = "wss://phish.sinking.yachts/feed";
    private const string UserAgent = "Phishing Gateway Helper V1 (github.com/VelvetThePanda/PhishingGateway)";
    private const int WebSocketBufferSize = 16 * 1024;

    private readonly ClientWebSocket _websocket;

    public PhishingGatewayService(IHttpClientFactory clientFactory)
    {
        _http = clientFactory.CreateClient("vtp-phishing");
        
        _websocket = new();
        _websocket.Options.SetRequestHeader(HeaderName, UserAgent);
    }
    
    
    public async Task<Result> StartAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);
        
        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.FromError(new HttpError(ApiUrl, HttpMethod.Get, (int)response.StatusCode));
       
        var content = await response.Content.ReadAsStringAsync();
        
        var json = JsonDocument.Parse(content);
        
        var domains = json.RootElement.GetProperty("domains").EnumerateArray();
        
        return Result.FromSuccess();
    }
    
    public async Task StopAsync(CancellationToken cancellationToken) { }

    private async Task FetchDomainsAsync()
    {
        
    }
}