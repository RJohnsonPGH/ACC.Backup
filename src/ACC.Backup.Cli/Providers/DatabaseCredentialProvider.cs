using System.Collections;
using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using ACC.Client.Authentication.Credentials;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Cli.Providers;

public sealed class DatabaseCredentialProvider(ILogger<DatabaseCredentialProvider> logger, ConfigurationDbContext dbContext) : ICredentialProvider
{
	private readonly Lazy<IEnumerable<ICredential>> _credentials = new(() =>
	{
		try
		{
			var credentials = dbContext.Credentials.ToList();
			logger.LogInformationProviderPopulatedConfiguration([$"{credentials.Count}"]);
			return credentials;
		}
		catch (Exception ex)
		{
			logger.LogCriticalProviderConfigurationFailed(ex, []);
			throw;
		}
	});

	public IEnumerator<ICredential> GetEnumerator() => _credentials.Value.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
