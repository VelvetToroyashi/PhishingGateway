using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Remora.Results;
using VTP.AntiPhishingGateway.Errors;

namespace VTP.AntiPhishingGateway;

public class PhishingGatewayService
{
    private HashSet<string> _domains = new();
    private readonly HttpClient _http;

    private const string HeaderName = "X-Identity";
    private const string ApiUrl = "https://phish.sinking.yachts/v2/all";
    private const string WebSocketUrl = "wss://phish.sinking.yachts/feed";
    private const string UserAgent = "Phishing Gateway Helper V1 (github.com/VelvetThePanda/PhishingGateway)";
    private const int WebSocketBufferSize = 16 * 1024;

    private readonly ClientWebSocket _websocket;


    private Task _receiverTask;
    private CancellationTokenSource _cts;


    public PhishingGatewayService(IHttpClientFactory clientFactory)
    {
        _http = clientFactory.CreateClient("vtp-phishing");

        _websocket = new();
        _websocket.Options.SetRequestHeader(HeaderName, UserAgent);
    }

    public bool IsKnownPhishingDomain(string domain) => _domains.Contains(domain);

    /// <summary>
    /// Fetches domains from REST and connects to the phishing websocket API.
    /// </summary>
    /// <returns>A result that may or not have succeeded.</returns>
    public async Task<Result> StartAsync(CancellationToken cancellationToken)
    {
        var domainResult = await FetchDomainsAsync(cancellationToken);

        if (!domainResult.IsSuccess)
            return domainResult;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _receiverTask = ReceiverLoopAsync();

        return Result.FromSuccess();
    }

    /// <summary>
    /// Disconnects from the phishing websocket API.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
    }

    private async Task ReceiverLoopAsync()
    {
        using var buffer = new ArrayPoolBufferWriter<byte>(WebSocketBufferSize);

        while (true)
        {
            await _websocket.ConnectAsync(new(WebSocketUrl), CancellationToken.None);

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    ValueWebSocketReceiveResult result;
                    do
                    {
                        Memory<byte> mem = buffer.GetMemory(WebSocketBufferSize);
                        result = await _websocket.ReceiveAsync(mem, _cts.Token);

                        if (result.MessageType is WebSocketMessageType.Close)
                            break; // CloudFlare occasionally kills idle sockets. //

                        buffer.Advance(result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType is WebSocketMessageType.Close)
                        break; // Attempt to reconnect. //

                    string? json = Encoding.UTF8.GetString(buffer.WrittenSpan);

                    var payload = JsonDocument.Parse(json);

                    var command = payload.RootElement.GetProperty("type").GetString();
                    var domains = payload.RootElement.GetProperty("domains").Deserialize<string[]>();

                    HandleWebsocketCommand(command, domains);

                    buffer.Clear();
                }
            }
            catch
            {
                await _websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Reconnecting!", CancellationToken.None);
            }

            if (_cts.Token.IsCancellationRequested)
            {
                try { await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopping!", CancellationToken.None); }
                catch { }

                return;
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(15)); // If we can't reconnect, don't spam, else CF might ban us. //
            }
        }

    }


    private void HandleWebsocketCommand(string? command, string[] domains)
    {
        switch (command)
        {
            case "add":
                foreach (string domain in domains)
                    _domains.Add(domain);
                break;

            case "delete":
                foreach (string domain in domains)
                    _domains.Remove(domain);
                break;

            default:
                //TODO: Log
                break;
        }
    }

    private async Task<Result> FetchDomainsAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);

        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.FromError(new HttpError(ApiUrl, HttpMethod.Get, (int)response.StatusCode));

        var content = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(content);

        var domains = json.Deserialize<string[]>();

        _domains = new(domains);
        
        return Result.FromSuccess();
    }

}