using ACC.Backup.Core.Data;
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
	/// <summary>
	/// Creates a new instance of the <see cref="RepositoryDbContext"/> using the current repository path.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException">Thrown if the repository has not been initialized with a path</exception>
	private async Task<RepositoryDbContext> CreateRepositoryContextAsync(CancellationToken cancellationToken)
	{
		var options = new DbContextOptionsBuilder<RepositoryDbContext>()
			.UseSqlite($"Data Source={Path.Combine(pathProvider.RepositoryPath, "repository.db")}")
			.Options;

		var context = new RepositoryDbContext(options);
		//await context.Database.EnsureCreatedAsync(cancellationToken);
		return context;
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
	public async Task<bool> BackupItemToRepositoryAsync(IProgress<double> progress, Item item, Uri signedUri, CancellationToken cancellationToken)
	{
		try
		{
			// Combine the root path with the file relative path and then ensure it is created
			var destinationPath = Path.Combine(pathProvider.RepositoryPath, item.ProjectId, item.Urn.Id[..5], item.Urn.Id, Path.GetFileName(signedUri.LocalPath));
			logger.LogTraceItemDownloadDestination(item.ProjectId, item.Id, item.Version, destinationPath);
			Directory.CreateDirectory(destinationPath);

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
			//await using var fileStream = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
			//await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);

			//var buffer = new byte[81920];
			//long totalRead = 0;
			//int read;
			//while ((read = await httpStream.ReadAsync(buffer, cancellationToken)) > 0)
			//{
			//	cancellationToken.ThrowIfCancellationRequested();

			//	await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
			//	totalRead += read;
			//	progress.Report(totalBytes.HasValue ? (double)totalRead / totalBytes.Value * 100 : 0);
			//}

			// Ensure progress is reported as 100% on completion
			progress.Report(100);
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
}
