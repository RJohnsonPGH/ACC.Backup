using ACC.Backup.Core.Backup.Progress;
using ACC.Backup.Core.Exclusion;
using ACC.Backup.Core.Reporting;
using ACC.Backup.Core.Repository;
using ACC.Backup.Core.Logging;
using Microsoft.Extensions.Logging;
using ACC.Client.Entities;
using ACC.Client;
using ACC.Backup.Core.Download;

namespace ACC.Backup.Core.Backup;

public sealed partial class BackupService(ILogger<BackupService> logger, 
	IExclusionService exclusionService, IAccApiClient client, IDownloadService downloadService, IRepository repositoryService, IReportingService reportService, IDegreeOfParallelismProvider degreeOfParallelismProvider) : IBackupService
{
	/// <summary>
	/// Enumerates all hubs in the tenant, reporting progress to the provided IProgress instance.
	/// </summary>
	/// <param name="progress">A Progress instance that will trigger when a hub is enumerated and when the enumeration has finished.</param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns>Returns a task that completes when the enumeration has finished.</returns>
	public async Task EnumerateHubsAsync(IProgress<DiscoveryProgress> progress, CancellationToken cancellationToken = default)
	{
		try
		{
			// Retrieve all hubs in the tenant
			logger.LogInformationChildEnumerationStart("Tenant", string.Empty, string.Empty);
			await foreach (var hub in client.GetHubsAsync(cancellationToken: cancellationToken))
			{
				// Check if the hub should be excluded
				if (exclusionService.ShouldExcludeItem(hub.Id))
				{
					reportService.AddHub(hub.Id, hub.Name, true);
					logger.LogInformationItemExcluded(hub.Id, hub.Name);
					continue;
				}

				// The hub is not excluded
				reportService.AddHub(hub.Id, hub.Name);
				await _hubs.Writer.WriteAsync(hub, cancellationToken);
				progress.Report(DiscoveryProgress.HubDiscovered);
				logger.LogTraceItemEnumerated("Hub", hub.Id, hub.Name);
			}

			// The current tenant has been fully enumerated
			progress.Report(DiscoveryProgress.TenantEnumerated);
			logger.LogInformationChildEnumerationComplete("Tenant", string.Empty, string.Empty);
		}
		finally
		{
			// Complete the hubs channel to signal that no more hubs will be written
			_hubs.Writer.Complete();
		}

		// All tenants have been enumerated
		// Note: There is only one tenant, but for consistency we keep the same flow as hubs and projects
		progress.Report(DiscoveryProgress.HubEnumerationComplete);
		logger.LogInformationEnumerationComplete("Tenant");
	}

	/// <summary>
	/// Enumerates all projects in all hubs, reporting progress to the provided IProgress instance.
	/// </summary>
	/// <param name="progress">A Progress instance that will trigger when a project is discovered, when a hub is enumerated, and when the enumeration has finished.</param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns>Returns a task that completes when the enumeration has finished.</returns>
	public async Task EnumerateProjectsAsync(IProgress<DiscoveryProgress> progress, CancellationToken cancellationToken = default)
	{
		try
		{
			// Retrieve all projects in all hubs in the tenant in parallel, based on the specified degree of parallelism
			await Parallel.ForEachAsync(
				_hubs.Reader.ReadAllAsync(cancellationToken),
				new ParallelOptions()
				{
					MaxDegreeOfParallelism = degreeOfParallelismProvider.DegreeOfParallelism,
					CancellationToken = cancellationToken
				},
				async (hub, token) =>
				{
					using var logScope = logger.BeginScope(new { hub.Id, hub.Name });

					// Retrieve all projects in the hub
					logger.LogInformationChildEnumerationStart("Hub", hub.Name, hub.Id);
					await foreach (var project in client.GetProjectsAsync(hub, token))
					{
						// Check if the project should be excluded
						if (exclusionService.ShouldExcludeItem(project.Id))
						{
							reportService.AddProject(project.Id, project.Name, hub.Id, true);
							logger.LogInformationItemExcluded(project.Id, project.Name);
							continue;
						}

						// The project is not excluded
						reportService.AddProject(project.Id, project.Name, hub.Id);
						await _projects.Writer.WriteAsync(project, cancellationToken);
						progress.Report(DiscoveryProgress.ProjectDiscovered);
						logger.LogTraceItemEnumerated(nameof(Project), project.Id, project.Name);
					}

					// The current hub has been fully enumerated
					progress.Report(DiscoveryProgress.HubEnumerated);
					logger.LogInformationChildEnumerationComplete("Hub", hub.Id, hub.Name);
				}
			);
		}
		finally
		{
			// Complete the projects channel to signal that no more projects will be written
			_projects.Writer.Complete();
		}

		// All hubs have been enumerated
		progress.Report(DiscoveryProgress.ProjectEnumerationComplete);
		logger.LogInformationEnumerationComplete("Hub");
	}

	/// <summary>
	/// Enumerates all files in all projects, reporting progress to the provided IProgress instance.
	/// </summary>
	/// <param name="progress">A Progress instance that will trigger when a file is discovered, a project is enumerated, and when the enumeration has finished.</param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns>Returns a task that completes when the enumeration has finished.</returns>
	public async Task EnumerateFilesAsync(IProgress<DiscoveryProgress> progress, CancellationToken cancellationToken = default)
	{
		try
		{
			// Retrieve all files in all projects in all hubs in the tenant in parallel, based on the specified degree of parallelism
			await Parallel.ForEachAsync(
				_projects.Reader.ReadAllAsync(cancellationToken),
				new ParallelOptions()
				{
					MaxDegreeOfParallelism = degreeOfParallelismProvider.DegreeOfParallelism,
					CancellationToken = cancellationToken
				},
				async (project, token) =>
				{
					using var logScope = logger.BeginScope(new { project.Id, project.Name });

					// Retrieve all files in the project
					logger.LogInformationChildEnumerationStart("Project", project.Name, project.Id);
					await foreach (var currentItem in client.GetProjectContentsAsync(project, token))
					{
						await _projectFiles.Writer.WriteAsync(currentItem, token);
						progress.Report(DiscoveryProgress.FileDiscovered);
						logger.LogTraceItemEnumerated(nameof(Item), project.Id, project.Name);
					}

					// The current project has been fully enumerated
					progress.Report(DiscoveryProgress.ProjectEnumerated);
					logger.LogInformationChildEnumerationComplete("Project", project.Name, project.Id);
				}
			);
		}
		finally
		{
			// Complete the project files channel to signal that no more files will be written
			_projectFiles.Writer.Complete();
		}

		// All projects have been enumerated
		progress.Report(DiscoveryProgress.FileEnumerationComplete);
		logger.LogInformationEnumerationComplete("Project");
	}

	/// <summary>
	/// Backs up all files in all projects, reporting progress to the provided IProgress instance.
	/// </summary>
	/// <param name="progress"></param>
	/// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
	/// <returns>Returns a task that completes when the enumeration has finished.</returns>
	public async Task BackupProjectFilesAsync(IProgress<BackupProgress> progress, CancellationToken cancellationToken = default)
	{
		// Backup all files in all projects in parallel, based on the specified degree of parallelism
		await Parallel.ForEachAsync(
			_projectFiles.Reader.ReadAllAsync(cancellationToken),
			new ParallelOptions()
			{
				MaxDegreeOfParallelism = degreeOfParallelismProvider.DegreeOfParallelism,
				CancellationToken = cancellationToken
			},
			async (item, token) =>
			{
				using var logScope = logger.BeginScope(new { item.Id, item.Name });

				// Check if file has a download URL, and fail it immediately if it does not
				if (item.DownloadUrl is null)
				{
					reportService.AddFile(item.Id, item.ProjectId, item.Name, ReportingState.Failed);
					reportService.AddMessage($"No download URI for item: {item.ProjectName} - {item.Name}");
					progress.Report(new()
					{
						Id = item.Id,
						Name = item.Name,
						PercentComplete = 0,
						Status = BackupProgress.BackupStatus.Failed
					});
					logger.LogErrorNoDownloadUri(item.Id, item.Name);
					return;
				}

				// Check if file is already backed up, and mark it as completed if it is
				var latestVersionInRepository = await repositoryService.GetItemVersionFromRepositoryAsync(item, token);
				if (item.Version <= latestVersionInRepository)
				{
					reportService.AddFile(item.Id, item.ProjectId, item.Name, ReportingState.UpToDate);
					progress.Report(new()
					{
						Id = item.Id,
						Name = item.Name,
						PercentComplete = 100,
						Status = BackupProgress.BackupStatus.Completed
					});
					logger.LogTraceDownloadSkipped(item.Id, item.Name, item.Version);
					return;
				}

				// The file needs to be backed up, so get a signed URL and back it up
				var downloadUri = new Uri(item.DownloadUrl);
				var signedDownloadUri = await client.GetSignedDownloadUriAsync(downloadUri, token);
				logger.LogDebugItemSignedUriRetrieved(item.Id, item.Name, item.Version, signedDownloadUri);

				var downloadProgress = new Progress<DownloadProgress>(x =>
				{
					progress.Report(new()
					{
						Id = item.Id,
						Name = item.Name,
						BytesDownloaded = x.BytesDownloaded,
						BytesTotal = x.BytesTotal,
						PercentComplete = x.BytesTotal == 0 ? 0 : (double)x.BytesDownloaded / x.BytesTotal * 100.0,
						Status = BackupProgress.BackupStatus.InProgress
					});
				});
				var tempPath = Path.GetTempFileName();
				var downloadResult = await downloadService.DownloadFileAsync(downloadProgress, signedDownloadUri, tempPath, token);

				// If the backup failed, report it and log it
				if (!downloadResult)
				{
					reportService.AddFile(item.Id, item.ProjectId, item.Name, ReportingState.Failed);
					progress.Report(new()
					{
						Id = item.Id,
						Name = item.Name,
						PercentComplete = 0,
						Status = BackupProgress.BackupStatus.Failed
					});
					logger.LogErrorFileDownloadFailed(item.Id, item.Name, item.Version);
					return;
				}
				logger.LogDebugFileDownloadComplete(item.Id, item.Name, item.Version);

				// Ingest the file into the repository
				var ingestResult = await repositoryService.IngestItemAsync(item, tempPath, token);
				if (!ingestResult)
				{
					reportService.AddFile(item.Id, item.ProjectId, item.Name, ReportingState.Failed);
					progress.Report(new()
					{
						Id = item.Id,
						Name = item.Name,
						PercentComplete = 0,
						Status = BackupProgress.BackupStatus.Failed
					});
					return;
				}

				// The backup was succesful, report it and log it
				reportService.AddFile(item.Id, item.ProjectId, item.Name, ReportingState.Successful);
				progress.Report(new()
				{
					Id = item.Id,
					Name = item.Name,
					PercentComplete = 100,
					Status = BackupProgress.BackupStatus.Completed
				});
				logger.LogDebugItemBackupComplete(item.Id, item.Name, item.Version);
			}
		);

		// All files have been backed up
		logger.LogInformationBackupComplete();
	}

	public Task SaveReportAsync(CancellationToken cancellationToken = default)
	{
		var reportHtml = reportService.GenerateReport();
		return repositoryService.SaveReportToRepositoryAsync(reportHtml, cancellationToken);
	}
}

