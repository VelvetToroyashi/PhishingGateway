using Remora.Results;

namespace VTP.AntiPhishingGateway;

public record PhishingDetectionResult(bool IsPhishing, PhishingSource Source);