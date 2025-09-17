using ACC.Client.Authentication.Credentials;

namespace ACC.Backup.Cli.Data;

/// <summary>
/// A simple implementation of <see cref="ICredential"/> to hold credentials for accessing the ACC API.
/// </summary>
public sealed record Credential : ICredential
{
	public required string Id { get; init; }
	public required string Secret { get; init; }

	public bool Equals(ICredential? other)
	{
		return string.Equals(other?.Id, Id, StringComparison.Ordinal) &&
			string.Equals(other?.Secret, Secret, StringComparison.Ordinal);
	}
}
