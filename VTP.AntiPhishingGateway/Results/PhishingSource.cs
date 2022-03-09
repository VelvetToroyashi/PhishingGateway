namespace VTP.AntiPhishingGateway;

/// <summary>
/// Where a phishing links was detected from.
/// </summary>
public enum PhishingSource
{
    /// <summary>
    /// The phishing link was detected from the gateway.
    /// </summary>
    PhishingGateway,
    
    /// <summary>
    /// The phishing link was detected via following a shortner link.
    /// </summary>
    PhishingShortner,

    /// <summary>
    /// The phishing link was detected via the aggregate phishing API.
    /// </summary>
    PhishingAggregateAPI
    
}