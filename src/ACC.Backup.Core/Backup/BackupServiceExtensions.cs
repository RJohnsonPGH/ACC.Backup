using Microsoft.Extensions.DependencyInjection;

namespace ACC.Backup.Core.Backup;

public static class BackupServiceExtensions
{
	public static IServiceCollection AddBackupService(this IServiceCollection services, Action<BackupServiceBuilder> configure)
	{
		var builder = new BackupServiceBuilder();
		configure(builder);

		if (builder.ExclusionServiceFactory is null)
		{
			throw new InvalidOperationException("You must register an IExclusionProvider.");
		}

		if (builder.DownloadServiceFactory is null)
		{
			throw new InvalidOperationException("You must register an IDownloadService.");
		}

		if (builder.RepositoryServiceFactory is null)
		{
			throw new InvalidOperationException("You must register an IRepositoryService");
		}

		if (builder.ReportingServiceFactory is null)
		{
			throw new InvalidOperationException("You must register an IReportingService");
		}

		services.AddSingleton(provider => builder.ExclusionServiceFactory(provider));
		services.AddSingleton(provider => builder.DownloadServiceFactory(provider));
		services.AddSingleton(provider => builder.RepositoryServiceFactory(provider));
		services.AddSingleton(provider => builder.ReportingServiceFactory(provider));
		services.AddSingleton<IBackupService, BackupService>();

		return services;
	}
}
