using System.Text.Json.Serialization;

namespace ACC.Client.RestApiResponses;

// https://aps.autodesk.com/en/docs/oauth/v2/reference/http/gettoken-POST/
/// <summary>
/// Response from the Authenticate endpoint
/// </summary>
public record AuthenticateResponse
{
	[JsonPropertyName("access_token")]
	[JsonRequired]
	public string BearerToken { get; init; } = string.Empty;

	[JsonPropertyName("expires_in")]
	[JsonRequired]
	public int ExpiresIn { get; init; }
}