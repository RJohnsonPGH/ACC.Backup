using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using ACC.Backup.Core.Backup;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Cli.Providers;

/// <summary>
/// Provides the degree of parallelism based on the number of credentials stored in the database.
/// </summary>
/// <param name="logger"></param>
/// <param name="dbContext"></param>
public sealed class DatabaseDegreeOfParallelismProvider(ILogger<DatabaseDegreeOfParallelismProvider> logger, ConfigurationDbContext dbContext) : IDegreeOfParallelismProvider
{
	private readonly Lazy<int> _credentialCount = new(() =>
	{
		try
		{
			var credentialCount = dbContext.Credentials.Count();
			logger.LogInformationProviderPopulatedConfiguration([$"{credentialCount}"]);
			return credentialCount;
		}
		catch (Exception ex)
		{
			logger.LogCriticalProviderConfigurationFailed(ex, []);
			throw;
		}
	});

	// Degree of parallelism is double the number of credentials, capped at 16
	public int DegreeOfParallelism => Math.Min(_credentialCount.Value * 2, 16);
}
