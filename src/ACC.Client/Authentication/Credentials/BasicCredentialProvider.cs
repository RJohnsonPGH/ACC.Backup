using System.Collections;

namespace ACC.Client.Authentication.Credentials;

/// <summary>
/// A basic implementation of ICredentialProvider that stores credentials in memory.
/// </summary>
public sealed class BasicCredentialProvider : ICredentialProvider
{
	private IEnumerable<ICredential> _credentials = [];

	public void AddCredentials(ICredential[] credentials)
	{
		_credentials = [.. _credentials.Concat(credentials).Distinct()];
	}

	public IEnumerator<ICredential> GetEnumerator() => _credentials.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
