namespace ACC.Client.Authentication.Credentials;

/// <summary>
/// Represents a provider of credentials for authenticating with the ACC API.
/// </summary>
public interface ICredentialProvider : IEnumerable<ICredential> { }
