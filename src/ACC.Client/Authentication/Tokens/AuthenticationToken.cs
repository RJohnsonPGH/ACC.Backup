namespace ACC.Client.Authentication.Tokens;

/// <summary>
/// Represents an authentication token with its associated metadata.
/// </summary>
internal sealed record AuthenticationToken
{
	internal required string ClientId { get; init; }
	internal string BearerToken { get; init; } = string.Empty;
	internal DateTime RefreshAfter { get; init; }
}
