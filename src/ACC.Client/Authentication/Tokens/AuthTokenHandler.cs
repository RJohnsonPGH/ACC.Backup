using System.Net.Http.Headers;
namespace ACC.Client.Authentication.Tokens;

/// <summary>
/// HTTP handler that adds authentication tokens to outgoing requests.
/// </summary>
/// <param name="tokenService"></param>
public sealed class AuthTokenHandler(ITokenService tokenService) : DelegatingHandler
{
	/// <summary>
	/// Sends an HTTP request with an authentication token added to the Authorization header.
	/// </summary>
	/// <param name="request">The request that will have the Authorization header added.</param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns>A Task containing the HttpResponseMessage.</returns>
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var token = await tokenService.GetNextBearerTokenAsync(cancellationToken);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		return await base.SendAsync(request, cancellationToken);
	}
}
