using ACC.Backup.Core.Data;
using ACC.Backup.Core.Download;
using ACC.Backup.Core.Logging;
using ACC.Client.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Repository;

/// <summary>
/// A simple local storage repository implementation using SQLite and the local file system.
/// </summary>
/// <param name="logger"></param>
/// <param name="client"></param>
public sealed partial class LocalStorageRepository(ILogger<LocalStorageRepository> logger, HttpClient client, ILocalStorageRepositoryPathProvider pathProvider) : IRepository
{
	private readonly Lazy<Task> _initializationTask = new(async () =>
	{
		var databasePath = Path.Combine(pathProvider.RepositoryPath, "repository.db");
		var options = new DbContextOptionsBuilder<RepositoryDbContext>()
			.UseSqlite($"Data Source={databasePath}")
			.Options;

		var context = new RepositoryDbContext(options);
		await context.Database.EnsureCreatedAsync();
	});

	/// <summary>
	/// Creates a new instance of the <see cref="RepositoryDbContext"/> using the current repository path.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">Thrown if the repository has not been initialized with a path</exception>
	private async Task<RepositoryDbContext> CreateRepositoryContextAsync(CancellationToken cancellationToken)
	{
		await _initializationTask.Value;

		var databasePath = Path.Combine(pathProvider.RepositoryPath, "repository.db");
		var options = new DbContextOptionsBuilder<RepositoryDbContext>()
			.UseSqlite($"Data Source={databasePath}")
			.Options;

		return new RepositoryDbContext(options);
	}

	/// <summary>
	/// Downloads the specified item from the provided signed URI, reporting progress to the provided IProgress instance.
	/// </summary>
	/// <param name="progress"></param>
	/// <param name="item"></param>
	/// <param name="signedUri"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">Thrown if the repository has not been initialized.</exception>
	public async Task<bool> BackupItemToRepositoryAsync(IProgress<DownloadProgress> progress, Item item, Uri signedUri, CancellationToken cancellationToken)
	{
		var tempPath = Path.GetTempFileName();
		try
		{
			// Perform the HTTP request and verify the response indicates success
			var response = await client.GetAsync(signedUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogErrorHttpRequestNotSuccesful(item.ProjectId, item.Id, item.Version, response.StatusCode);
				return false;
			}

			// Get the total file size from the Content-Length header if available
			var totalBytes = response.Content.Headers.ContentLength;
			logger.LogTraceBackupFileSize(item.ProjectId, item.Id, item.Version, totalBytes ?? 0);

			// Download the file in chunks, reporting progress
			await using var fileStream = File.Open(tempPath, FileMode.Create);
			await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			var buffer = new byte[81920];
			long totalRead = 0;
			int read;
			while ((read = await httpStream.ReadAsync(buffer, cancellationToken)) > 0)
			{
				cancellationToken.ThrowIfCancellationRequested();

				await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
				totalRead += read;
				progress.Report(new(totalRead, totalBytes ?? 0));
			}
#warning refactor download service into separate service
			await fileStream.DisposeAsync();

			// Combine the root path with the file relative path and then ensure it is created
			var destinationFolder = Path.Combine(pathProvider.RepositoryPath, item.ProjectId, item.Urn.Id, $"{item.Version}");
			var destinationPath = Path.Combine(destinationFolder, item.Name);
			logger.LogTraceItemDownloadDestination(item.ProjectId, item.Id, item.Version, destinationPath);
			Directory.CreateDirectory(destinationFolder);
			File.Move(tempPath, destinationPath, true);

			// Update the file creation and modification times to match the item's last modified time
			File.SetCreationTimeUtc(destinationPath, item.LastModifiedTime);
			File.SetLastWriteTimeUtc(destinationPath, item.LastModifiedTime);

			logger.LogTraceItemDownloadedSuccessfully(item.ProjectId, item.Id, item.Version);

			// Update the database with the new item version
			using var dbContext = await CreateRepositoryContextAsync(cancellationToken);
			var repoItem = await dbContext.Items.FindAsync([item.Id], cancellationToken);

			// If the item does not exist, insert it as a new record
			if (repoItem is null)
			{
				dbContext.Items.Add(new()
				{
					Id = item.Id,
					ProjectId = item.ProjectId,
					ProjectName = item.ProjectName,
					FolderId = item.FolderId,
					Name = item.Name,
					LatestVersion = item.Version
				});
			}
			else
			{
				// While the ID doesnt change, the name or project name might
				repoItem.Name = item.Name;
				repoItem.ProjectName = item.ProjectName;
				repoItem.LatestVersion = item.Version;
			}

			await dbContext.SaveChangesAsync(cancellationToken);
			logger.LogTraceItemMetadataUpdated(item.ProjectId, item.Id, item.Version);

			return true;
		}
		catch (Exception ex)
		{
			File.Delete(tempPath);
			logger.LogErrorItemDownloadFailed(ex, item.ProjectId, item.Id, item.Version);
			return false;
		}
	}

	/// <summary>
	/// Gets the latest version number of the specified item from the repository database.
	/// </summary>
	/// <param name="itemId">The item identifier</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An int of the current version of the item, and 0 if it does not exist.</returns>
	public async Task<int> GetItemVersionFromRepositoryAsync(Item item, CancellationToken cancellationToken)
	{
		using var dbContext = await CreateRepositoryContextAsync(cancellationToken);
		var repoItem = await dbContext.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id, cancellationToken);

		// Item is not in database, all versions should be backed up
		if (repoItem is null)
		{
			logger.LogTraceItemRepositoryVersion(item.ProjectId, item.Id, 0);
			return 0;
		}

		logger.LogTraceItemRepositoryVersion(item.ProjectId, item.Id, repoItem.LatestVersion);
		return repoItem.LatestVersion;
	}

	public async Task<bool> SaveReportToRepositoryAsync(string reportHtml, CancellationToken cancellationToken)
	{
		try
		{
			var reportDirectory = Path.Combine(pathProvider.RepositoryPath, "reports");
			var reportPath = Path.Combine(reportDirectory, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.html");
			
			Directory.CreateDirectory(reportDirectory);
			await File.WriteAllTextAsync(reportPath, reportHtml, cancellationToken);

			logger.LogInformationReportSaved(reportPath);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogErrorReportSaveFailed(ex);
			return false;
		}
	}
}
