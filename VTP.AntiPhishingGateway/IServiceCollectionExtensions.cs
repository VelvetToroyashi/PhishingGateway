using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace VTP.AntiPhishingGateway;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds a hosted service to start <see cref="AntiPhishingGateway"/>. on startup.
    /// </summary>
    public static IServiceCollection AddPhishingService(this IServiceCollection collection)
    {
        collection.ConfigureOptions<PhishingServiceOptions>();
        collection.AddHostedService<PhishingService>();
        
        return collection;
    }
    
    
    /// <summary>
    /// Adds requisite services for phishing detection to the service collection.
    /// </summary>
    /// <param name="collection">The container to add services to.</param>
    /// <returns>The container to chain calls with.</returns>
    public static IServiceCollection AddAntiPhishing(this IServiceCollection collection)
    {
        collection.AddHttpClient(Constants.HttpClientName, client =>
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(Constants.IdentityHeader, Constants.ProjectIdentifier);
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Constants.ProjectIdentifier);
            });

        collection.ConfigureOptions<PhishingDetectionOptions>();
        
        collection.TryAddSingleton<PhishingGatewayService>();
        collection.TryAddSingleton<PhishingDetectionService>();
        
        return collection;
    }
}