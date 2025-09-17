using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using ACC.Backup.Core.Repository;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Cli.Providers;

public sealed class DatabaseLocalStorageRepositoryPathProvider(ILogger<DatabaseLocalStorageRepositoryPathProvider> logger, ConfigurationDbContext dbContext, JobIdProvider jobIdProvider) : ILocalStorageRepositoryPathProvider
{
	public string RepositoryPath => _repositoryPath.Value;

	private readonly Lazy<string> _repositoryPath = new(() =>
	{
		try
		{
			var job = dbContext.Jobs.SingleOrDefault(x => x.Id == jobIdProvider.JobId) ??
				throw new InvalidOperationException($"No job found with Id = {jobIdProvider.JobId}.");

			logger.LogInformationProviderPopulatedConfiguration([job.StoragePath]);
			return job.StoragePath;
		}
		catch (Exception ex)
		{
			logger.LogCriticalProviderConfigurationFailed(ex, [$"{jobIdProvider.JobId}"]);
			throw;
		}
	});
}
