
namespace ACC.Client.Authentication.Tokens;

/// <summary>
/// Represents a service for obtaining bearer tokens for authenticating with the ACC API.
/// </summary>
public interface ITokenService
{
	Task<string> GetNextBearerTokenAsync(CancellationToken cancellationToken = default);
}
