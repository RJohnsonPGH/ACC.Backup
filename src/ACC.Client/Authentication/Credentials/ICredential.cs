namespace ACC.Client.Authentication.Credentials;

/// <summary>
/// Represents a set of credentials used for authenticating with the ACC API.
/// </summary>
public interface ICredential : IEquatable<ICredential>
{
	string Id { get; }
	string Secret { get; }
}
