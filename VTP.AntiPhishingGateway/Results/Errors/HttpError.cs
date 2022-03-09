using Remora.Results;

namespace VTP.AntiPhishingGateway.Errors;

/// <summary>
/// Represents an error when attempting to access a resource over HTTP.
/// </summary>
/// <param name="Url">The URL that was accessed.</param>
/// <param name="Method">The method used on the URL.</param>
/// <param name="ResposneCode">The status code returned from the remote server.</param>
public record HttpError(string Url, HttpMethod Method, int ResposneCode) : IResultError
{
    public string Message => $"{Method} {Url} returned {ResposneCode}";
}