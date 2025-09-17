using System.Text.Json.Serialization;

namespace ACC.Client.RestApiResponses;

/// <summary>
/// Response from the Signed URL endpoint
/// </summary>
public sealed record SignedUrlResponse
{
	[JsonPropertyName("url")]
	[JsonRequired] 
	public required string Url { get; init; }
}
