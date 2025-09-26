using Microsoft.Extensions.DependencyInjection;
using ACC.Backup.Core.Exclusion;
using ACC.Backup.Core.Repository;
using ACC.Client;
using ACC.Backup.Core.Reporting;
using ACC.Backup.Core.Download;

namespace ACC.Backup.Core.Backup;

public sealed class BackupServiceBuilder
{
	internal Func<IServiceProvider, IAccApiClient>? AccApiClientFactory { get; private set; }
	internal Func<IServiceProvider, IDownloadService>? DownloadServiceFactory { get; private set; }
	internal Func<IServiceProvider, IRepository>? RepositoryServiceFactory { get; private set; }
	internal Func<IServiceProvider, IExclusionService>? ExclusionServiceFactory { get; private set; }
	internal Func<IServiceProvider, IReportingService>? ReportingServiceFactory { get; private set; }

	public BackupServiceBuilder WithAccApiClient<T>() where T : class, IAccApiClient
	{
		AccApiClientFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public BackupServiceBuilder WithAccApiClient(IAccApiClient accApiClient)
	{
		AccApiClientFactory = _ => accApiClient;
		return this;
	}

	public BackupServiceBuilder WithAccApiClient(Func<IServiceProvider, IAccApiClient> factory)
	{
		AccApiClientFactory = factory;
		return this;
	}

	// Download Service
	public BackupServiceBuilder WithDownloadService<T>() where T : class, IDownloadService
	{
		DownloadServiceFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public BackupServiceBuilder WithDownloadService(IDownloadService downloadService)
	{
		DownloadServiceFactory = _ => downloadService;
		return this;
	}

	public BackupServiceBuilder WithDownloadService(Func<IServiceProvider, IDownloadService> factory)
	{
		DownloadServiceFactory = factory;
		return this;
	}


	// Repository Service
	public BackupServiceBuilder WithRepositoryService<T>() where T : class, IRepository
	{
		RepositoryServiceFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public BackupServiceBuilder WithRepositoryService(IRepository repositoryService)
	{
		RepositoryServiceFactory = _ => repositoryService;
		return this;
	}

	public BackupServiceBuilder WithRepositoryService(Func<IServiceProvider, IRepository> factory)
	{
		RepositoryServiceFactory = factory;
		return this;
	}

	// Exclusion Service
	public BackupServiceBuilder WithExclusionService<T>() where T : class, IExclusionService
	{
		ExclusionServiceFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public BackupServiceBuilder WithExclusionService(IExclusionService exclusionProvider)
	{
		ExclusionServiceFactory = _ => exclusionProvider;
		return this;
	}

	public BackupServiceBuilder WithExclusionService(Func<IServiceProvider, IExclusionService> factory)
	{
		ExclusionServiceFactory = factory;
		return this;
	}

	// Reporting Service
	public BackupServiceBuilder WithReportingService<T>() where T : class, IReportingService
	{
		ReportingServiceFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public BackupServiceBuilder WithReportingService(IReportingService reportingService)
	{
		ReportingServiceFactory = _ => reportingService;
		return this;
	}

	public BackupServiceBuilder WithReportingService(Func<IServiceProvider, IReportingService> factory)
	{
		ReportingServiceFactory = factory;
		return this;
	}
}
