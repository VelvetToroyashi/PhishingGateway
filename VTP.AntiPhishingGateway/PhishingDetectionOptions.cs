namespace VTP.AntiPhishingGateway;

/// <summary>
/// Options regarding how to detect phishing.
/// </summary>
/// <param name="FollowShortners">Whether to follow shortner links (such as bit.ly)</param>
/// <param name="UseSecondOpinionScanning">Whether to double-check messages against a second-opinion scan (anti-fish.bitflow.dev).</param>
public record PhishingDetectionOptions(bool FollowShortners, bool UseSecondOpinionScanning);