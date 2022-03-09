using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace VTP.AntiPhishingGateway;

public class PhishingService : IHostedService
{
    private readonly PhishingServiceOptions _options;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly PhishingGatewayService _gateway;

    public PhishingService(IOptions<PhishingServiceOptions> options, IHostApplicationLifetime lifetime, PhishingGatewayService gateway)
    {
        _options = options.Value;
        _lifetime = lifetime;
        _gateway = gateway;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var startupResult = await _gateway.StartAsync(cancellationToken);
        
        if (!startupResult.IsSuccess && _options.TerminateOnError)
            _lifetime.StopApplication();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _gateway.StopAsync(cancellationToken);
    }
}