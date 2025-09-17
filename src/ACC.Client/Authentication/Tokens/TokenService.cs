using System.Text;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using ACC.Client.RestApiResponses;
using ACC.Client.Authentication.Credentials;
using ACC.Client.Logging;
using System.Collections.Concurrent;

namespace ACC.Client.Authentication.Tokens;

/// <summary>
/// A service for obtaining bearer tokens for authenticating with the ACC API.
/// </summary>
/// <param name="logger"></param>
/// <param name="client"></param>
/// <param name="credentialProvider"></param>
public sealed class TokenService(ILogger<TokenService> logger, HttpClient client, ICredentialProvider credentialProvider) : ITokenService
{
	private readonly Uri _authenticationUri= new("https://developer.api.autodesk.com/authentication/v2/token");

	private static FormUrlEncodedContent RequestBody => new(
		new Dictionary<string, string> {
			{ "grant_type", "client_credentials" },
			{ "scope", "data:read" }
		}
	);

	private readonly Lazy<ICredential[]> _credentials = new(() =>
	{
		var credentials = credentialProvider.ToArray();
		if (credentials.Length == 0)
		{
			throw new InvalidOperationException("No credentials were provided.");
		}
		return credentials;
	}, isThreadSafe: true);
	private readonly ConcurrentDictionary<string, AuthenticationToken> _tokens = new();

	private int _index = -1;

	/// <summary>
	/// Gets the next access token asynchronously, refreshing it if necessary.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>The next bearer token.</returns>
	public async Task<string> GetNextBearerTokenAsync(CancellationToken cancellationToken = default)
	{
		var credentials = _credentials.Value;
		var index = Interlocked.Increment(ref _index);
		var rationalizedIndex = index % credentials.Length;
		var credential = credentials[rationalizedIndex];

		if (!_tokens.TryGetValue(credential.Id, out var token) ||
			DateTime.UtcNow >= token.RefreshAfter)
		{
			token = await RefreshTokenAsync(credential.Id, cancellationToken);
			_tokens[credential.Id] = token;
		}

		logger.LogTraceRetrievedToken(token.ClientId, token.RefreshAfter);
		return token.BearerToken;
	}

	/// <summary>
	/// Refreshes the bearer token for the specified client ID.
	/// </summary>
	/// <param name="clientId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	private async Task<AuthenticationToken> RefreshTokenAsync(string clientId, CancellationToken cancellationToken)
	{
		var credential = credentialProvider.Single(x => x.Id == clientId);
		var clientIdAndSecret = $"{credential.Id}:{credential.Secret}";
		var base64ClientIdAndSecretBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientIdAndSecret));

		using var request = new HttpRequestMessage(HttpMethod.Post, _authenticationUri);
		request.Headers.Authorization = new("Basic", base64ClientIdAndSecretBytes);
		request.Content = RequestBody;

		var response = await client.SendAsync(request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
			logger.LogCriticalRefreshTokenFailed(response.StatusCode.ToString(), errorContent);
			throw new HttpRequestException($"Failed to refresh bearer token. Status: {response.StatusCode}. Response: {errorContent}");
		}

		var result = await response.Content.ReadFromJsonAsync<AuthenticateResponse>(cancellationToken)
			?? throw new InvalidOperationException("Failed to deserialize authentication response.");

		logger.LogDebugRefreshedToken(clientId, result.ExpiresIn);

		return new()
		{
			ClientId = credential.Id,
			BearerToken = result.BearerToken,
			// Reduce the expiration time by 10% so we can refresh it before it actually expires
			RefreshAfter = DateTime.UtcNow.AddSeconds(result.ExpiresIn * 0.9)
		};
	}
}
