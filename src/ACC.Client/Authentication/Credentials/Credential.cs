namespace ACC.Client.Authentication.Credentials;

/// <summary>
/// Represents a set of credentials used for authenticating with the ACC API.
/// </summary>
public sealed record Credential : ICredential
{
	public required string Id { get; init; }
	public required string Secret { get; init; }

	public bool Equals(ICredential? other)
	{
		return string.Equals(other?.Id, Id) && 
			string.Equals(other?.Secret, Secret);
	}
}
