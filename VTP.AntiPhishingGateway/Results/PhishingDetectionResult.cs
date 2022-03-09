using Remora.Results;

namespace VTP.AntiPhishingGateway;

public record PhishingDetectionResult(bool IsPhishing, IReadOnlyDictionary<string, PhishingSource>? DetectedDomains);