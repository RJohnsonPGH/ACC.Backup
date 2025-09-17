using ACC.Backup.Cli.Data;
using ACC.Backup.Cli.Logging;
using ACC.Backup.Core.Exclusion;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Cli.Providers;

public sealed class DatabaseExclusionProvider(ILogger<DatabaseExclusionProvider> logger, ConfigurationDbContext dbContext, JobIdProvider jobIdProvider) : IExclusionProvider
{
	public string[] IncludedIds => _job.Value.IncludeIds;
	public string[] ExcludedIds => _job.Value.ExcludeIds;

	private readonly Lazy<Job> _job = new(() =>
	{
		try
		{
			var job = dbContext.Jobs.SingleOrDefault(x => x.Id == jobIdProvider.JobId) ??
				throw new InvalidOperationException($"No job found with Id = {jobIdProvider.JobId}.");

			logger.LogInformationProviderPopulatedConfiguration([$"{job.IncludeIds.Length}", $"{job.ExcludeIds.Length}"]);
			return job;
		}
		catch (Exception ex)
		{
			logger.LogCriticalProviderConfigurationFailed(ex, [$"{jobIdProvider.JobId}"]);
			throw;
		}
	});
}
